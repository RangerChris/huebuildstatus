using System.Diagnostics;
using System.Text;
using System.Text.Json;
using HueApi;
using HueApi.ColorConverters;
using HueApi.ColorConverters.Original.Extensions;
using HueApi.Models;
using HueApi.Models.Clip;
using HueApi.Models.Requests;
using HueBuildStatus.Core.Features.Config;

namespace HueBuildStatus.Core.Features.Hue;

public class HueDiscoveryService(HttpClient? httpClient = null, string? discoveryUrl = null, IAppConfiguration? config = null) : IHueDiscoveryService
{
    private readonly string _discoveryUrl = discoveryUrl ?? "https://discovery.meethue.com/";
    private readonly HttpClient _httpClient = httpClient ?? new HttpClient();

    public async Task<string?> DiscoverBridgeAsync()
    {
        // If configured explicitly, prefer that
        if (!string.IsNullOrWhiteSpace(config?.BridgeIp))
        {
            return config!.BridgeIp;
        }

        try
        {
            using var resp = await _httpClient.GetAsync(_discoveryUrl);
            resp.EnsureSuccessStatusCode();
            var content = await resp.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            foreach (var el in doc.RootElement.EnumerateArray())
            {
                if (el.TryGetProperty("internalipaddress", out var ipProp) && ipProp.ValueKind == JsonValueKind.String)
                {
                    var ip = ipProp.GetString();
                    if (!string.IsNullOrEmpty(ip))
                    {
                        return ip;
                    }
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<RegisterEntertainmentResult?> Register(string bridgeIp, string hueUser)
    {
        if (string.IsNullOrWhiteSpace(bridgeIp))
        {
            throw new ArgumentNullException(nameof(bridgeIp));
        }

        if (string.IsNullOrWhiteSpace(hueUser))
        {
            throw new ArgumentNullException(nameof(hueUser));
        }

        try
        {
            // Use the static helper to register a new user on the bridge
            var result = await LocalHueApi.RegisterAsync(bridgeIp, hueUser, "pc");
            return result;
        }
        catch
        {
            return null;
        }
    }

    public async Task SetColorOfLamp(Light light, RGBColor color)
    {
        ArgumentNullException.ThrowIfNull(light);

        var client = await CreateClientAsync();
        if (client == null)
        {
            return;
        }

        var req = new UpdateLight()
            .TurnOn()
            .SetBrightness(100)
            .SetColor(color);

        await client.Light.UpdateAsync(light.Id, req);
    }

    public async Task SetColorOfLamp(Guid lightId, RGBColor color)
    {
        var client = await CreateClientAsync();
        if (client == null)
        {
            return;
        }

        var req = new UpdateLight()
            .TurnOn()
            .SetBrightness(100)
            .SetColor(color);

        await client.Light.UpdateAsync(lightId, req);
    }

    public async Task SetOnState(Guid lightId, bool on)
    {
        var client = await CreateClientAsync();
        if (client == null)
        {
            return;
        }

        var req = new UpdateLight();
        if (on)
        {
            req.TurnOn();
        }
        else
        {
            req.TurnOff();
        }

        await client.Light.UpdateAsync(lightId, req);
    }

    public async Task PulsateAsync(Light light, RGBColor color, int cycles = 3, int periodMs = 1000, int steps = 20, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(light);

        var client = await CreateClientAsync();
        if (client == null)
        {
            return;
        }

        if (steps <= 0)
        {
            steps = 20;
        }

        if (periodMs < 100)
        {
            periodMs = 100;
        }

        var id = light.Id;
        const int minBrightness = 1;
        const int maxBrightness = 100;

        // Get initial brightness
        var initialBrightness = light.Dimming?.Brightness;
        if (!initialBrightness.HasValue)
        {
            var latest = await client.Light.GetByIdAsync(id);
            initialBrightness = latest.Data.FirstOrDefault()?.Dimming?.Brightness;
        }

        var initialBriByte = initialBrightness.HasValue ? (byte?)Math.Round(initialBrightness.Value) : null;
        var halfStepDelay = periodMs / 2 / steps;

        for (var cycle = 0; cycle < cycles; cycle++)
        {
            // ramp up
            for (var s = 0; s <= steps; s++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var bri = (byte)(minBrightness + (maxBrightness - minBrightness) * s / (double)steps);
                var req = new UpdateLight()
                    .TurnOn()
                    .SetColor(color)
                    .SetBrightness(bri);
                await client.Light.UpdateAsync(id, req);
                await Task.Delay(halfStepDelay, cancellationToken);
            }

            // ramp down
            for (var s = steps; s >= 0; s--)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var bri = (byte)(minBrightness + (maxBrightness - minBrightness) * s / (double)steps);
                var req = new UpdateLight()
                    .TurnOn()
                    .SetColor(color)
                    .SetBrightness(bri);
                await client.Light.UpdateAsync(id, req);
                await Task.Delay(halfStepDelay, cancellationToken);
            }
        }

        // Restore initial brightness
        if (initialBriByte.HasValue)
        {
            var restoreReq = new UpdateLight()
                .TurnOn()
                .SetColor(color)
                .SetBrightness(initialBriByte.Value);
            await client.Light.UpdateAsync(id, restoreReq);
        }
    }

    public async Task<Dictionary<Guid, string>?> GetAllLights()
    {
        using var activity = new ActivitySource(TracingConstants.ActivitySourceName).StartActivity(nameof(GetAllLights), ActivityKind.Internal);
        activity?.SetTag("Core", "true");
        var client = await CreateClientAsync();
        if (client == null)
        {
            return null;
        }

        var lights = await client.Light.GetAllAsync();
        var dict = new Dictionary<Guid, string>();
        foreach (var light in lights.Data)
        {
            dict[light.Id] = light.Metadata?.Name ?? "Unknown";
        }

        return dict;
    }

    public async Task<LightSnapshot> CaptureLightSnapshotAsync(Guid lightId)
    {
        var client = await CreateClientAsync();
        if (client == null)
        {
            throw new InvalidOperationException("Hue client not available (missing bridge IP or key)");
        }

        var latest = await client.Light.GetByIdAsync(lightId);
        var light = latest.Data.FirstOrDefault();
        if (light == null)
        {
            throw new InvalidOperationException($"No light found with id {lightId}");
        }

        var json = JsonSerializer.Serialize(light, new JsonSerializerOptions { WriteIndented = false });
        return new LightSnapshot(lightId, json);
    }

    public async Task RestoreLightSnapshotAsync(LightSnapshot snapshot, int transitionMs = 0)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var client = await CreateClientAsync();
        if (client == null)
        {
            throw new InvalidOperationException("Hue client not available (missing bridge IP or key)");
        }

        using var doc = JsonDocument.Parse(snapshot.JsonSnapshot);
        var root = doc.RootElement;

        // Extract on/off state
        var isOn = ExtractOnState(root);

        // Extract brightness
        double? brightness = null;
        if (root.TryGetProperty("dimming", out var dimming) && dimming.ValueKind == JsonValueKind.Object)
        {
            if (dimming.TryGetProperty("brightness", out var briProp) && briProp.ValueKind == JsonValueKind.Number)
            {
                brightness = briProp.GetDouble();
            }
        }

        // Attempt to extract RGB color
        var rgb = ExtractRgbFromSnapshot(root, brightness);

        var req = new UpdateLight();
        var setAny = false;

        if (isOn.HasValue)
        {
            setAny = true;
            if (isOn.Value)
            {
                req.TurnOn();
            }
            else
            {
                req.TurnOff();
            }
        }

        if (brightness.HasValue)
        {
            setAny = true;
            var briVal = Math.Clamp((int)Math.Round(brightness.Value), 0, 100);
            req.SetBrightness((byte)briVal);
        }

        if (rgb is RGBColor rc)
        {
            setAny = true;
            try
            {
                req.SetColor(rc);
            }
            catch
            {
                // ignore
            }
        }

        if (!setAny)
        {
            return;
        }

        if (transitionMs > 0)
        {
            // ignore transition for now
        }

        try
        {
            await client.Light.UpdateAsync(snapshot.LightId, req);
        }
        catch
        {
            // swallow hardware errors
        }
    }

    public async Task<string?> AuthenticateAsync(string bridgeIp, string deviceType)
    {
        try
        {
            var url = $"http://{bridgeIp}/api";
            var payload = JsonSerializer.Serialize(new { devicetype = deviceType, generateclientkey = true });
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
            using var resp = await _httpClient.PostAsync(url, content);
            resp.EnsureSuccessStatusCode();
            var body = await resp.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in doc.RootElement.EnumerateArray())
                    {
                        if (el.TryGetProperty("success", out var successProp) && successProp.ValueKind == JsonValueKind.Object)
                        {
                            if (successProp.TryGetProperty("username", out var userProp) && userProp.ValueKind == JsonValueKind.String)
                            {
                                return userProp.GetString();
                            }
                        }

                        if (el.TryGetProperty("error", out var errorProp))
                        {
                            return null;
                        }
                    }
                }
            }
            catch
            {
                // ignore structured parsing failures and try simple text extraction below
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private async Task<LocalHueApi?> CreateClientAsync()
    {
        var bridgeIp = config?.BridgeIp;
        if (string.IsNullOrWhiteSpace(bridgeIp))
        {
            bridgeIp = await DiscoverBridgeAsync();
        }

        if (string.IsNullOrWhiteSpace(bridgeIp))
        {
            return null;
        }

        var appKey = config?.BridgeKey ?? Environment.GetEnvironmentVariable("HUE_APP_KEY");
        if (string.IsNullOrWhiteSpace(appKey))
        {
            return null;
        }

        return new LocalHueApi(bridgeIp, appKey);
    }

    private static bool? ExtractOnState(JsonElement root)
    {
        if (root.TryGetProperty("on", out var onProp))
        {
            var b = FindBooleanInElement(onProp, 5);
            if (b.HasValue)
            {
                return b;
            }
        }

        if (root.TryGetProperty("powerup", out var powerup) && powerup.ValueKind == JsonValueKind.Object)
        {
            if (powerup.TryGetProperty("on", out var puOn))
            {
                var b = FindBooleanInElement(puOn, 5);
                if (b.HasValue)
                {
                    return b;
                }
            }
        }

        return FindBooleanByName(root, "on", 5);
    }

    private static bool? FindBooleanInElement(JsonElement el, int maxDepth)
    {
        if (el.ValueKind == JsonValueKind.True || el.ValueKind == JsonValueKind.False)
        {
            return el.GetBoolean();
        }

        if (maxDepth <= 0)
        {
            return null;
        }

        if (el.ValueKind == JsonValueKind.Object)
        {
            if (el.TryGetProperty("on", out var inner))
            {
                var b = FindBooleanInElement(inner, maxDepth - 1);
                if (b.HasValue)
                {
                    return b;
                }
            }

            foreach (var prop in el.EnumerateObject())
            {
                var b = FindBooleanInElement(prop.Value, maxDepth - 1);
                if (b.HasValue)
                {
                    return b;
                }
            }
        }

        if (el.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in el.EnumerateArray())
            {
                var b = FindBooleanInElement(item, maxDepth - 1);
                if (b.HasValue)
                {
                    return b;
                }
            }
        }

        return null;
    }

    private static bool? FindBooleanByName(JsonElement root, string name, int maxDepth)
    {
        if (maxDepth <= 0)
        {
            return null;
        }

        if (root.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        foreach (var prop in root.EnumerateObject())
        {
            if (prop.NameEquals(name) && (prop.Value.ValueKind == JsonValueKind.True || prop.Value.ValueKind == JsonValueKind.False))
            {
                return prop.Value.GetBoolean();
            }

            if (prop.Value.ValueKind == JsonValueKind.Object)
            {
                var b = FindBooleanByName(prop.Value, name, maxDepth - 1);
                if (b.HasValue)
                {
                    return b;
                }
            }
        }

        return null;
    }

    private static RGBColor? ExtractRgbFromSnapshot(JsonElement root, double? brightness)
    {
        try
        {
            if (root.TryGetProperty("color", out var colorElem) && colorElem.ValueKind == JsonValueKind.Object)
            {
                if (colorElem.TryGetProperty("hex", out var hexProp) && hexProp.ValueKind == JsonValueKind.String)
                {
                    var hex = hexProp.GetString();
                    if (!string.IsNullOrWhiteSpace(hex))
                    {
                        try
                        {
                            return new RGBColor(hex.TrimStart('#'));
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }

                if (colorElem.TryGetProperty("spectrumRgb", out var srgb) && srgb.ValueKind == JsonValueKind.Array && srgb.GetArrayLength() == 3)
                {
                    try
                    {
                        var r = srgb[0].GetInt32();
                        var g = srgb[1].GetInt32();
                        var b = srgb[2].GetInt32();
                        return new RGBColor(r, g, b);
                    }
                    catch
                    {
                        // ignored
                    }
                }

                if (colorElem.TryGetProperty("xy", out var xy))
                {
                    double? xVal = null, yVal = null;
                    if (xy.ValueKind == JsonValueKind.Object)
                    {
                        if (xy.TryGetProperty("x", out var xp) && xp.ValueKind == JsonValueKind.Number)
                        {
                            xVal = xp.GetDouble();
                        }

                        if (xy.TryGetProperty("y", out var yp) && yp.ValueKind == JsonValueKind.Number)
                        {
                            yVal = yp.GetDouble();
                        }
                    }
                    else if (xy.ValueKind == JsonValueKind.Array && xy.GetArrayLength() >= 2)
                    {
                        try
                        {
                            xVal = xy[0].GetDouble();
                            yVal = xy[1].GetDouble();
                        }
                        catch
                        {
                            // ignored
                        }
                    }

                    try
                    {
                        if (xVal.HasValue && yVal.HasValue)
                        {
                            var x = xVal.Value;
                            var y = yVal.Value;
                            var yy = brightness.HasValue ? Math.Clamp(brightness.Value / 100.0, 0.0, 1.0) : 1.0;
                            if (y <= 0)
                            {
                                return null;
                            }

                            var X = yy / y * x;
                            var z = yy / y * (1 - x - y);

                            var rLin = 3.2406 * X - 1.5372 * yy - 0.4986 * z;
                            var gLin = -0.9689 * X + 1.8758 * yy + 0.0415 * z;
                            var bLin = 0.0557 * X - 0.2040 * yy + 1.0570 * z;

                            double GammaCorrect(double channel)
                            {
                                channel = Math.Max(0.0, channel);
                                if (channel <= 0.0031308)
                                {
                                    return 12.92 * channel;
                                }

                                return 1.055 * Math.Pow(channel, 1.0 / 2.4) - 0.055;
                            }

                            var r = (int)Math.Round(Math.Clamp(GammaCorrect(rLin), 0.0, 1.0) * 255.0);
                            var g = (int)Math.Round(Math.Clamp(GammaCorrect(gLin), 0.0, 1.0) * 255.0);
                            var b = (int)Math.Round(Math.Clamp(GammaCorrect(bLin), 0.0, 1.0) * 255.0);

                            return new RGBColor(r, g, b);
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            // Fallback: scan for string hex values
            foreach (var prop in root.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.String)
                {
                    var s = prop.Value.GetString();
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        var cleaned = s.Trim();
                        if (cleaned.StartsWith("#"))
                        {
                            cleaned = cleaned.Substring(1);
                        }

                        if (cleaned.Length == 6)
                        {
                            try
                            {
                                return new RGBColor(cleaned);
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // ignore
        }

        return null;
    }
}