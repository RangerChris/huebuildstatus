using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HueApi.ColorConverters;
using HueApi.Models;
using HueApi.Models.Clip;

namespace HueBuildStatus.Core.Features.Hue;

public class HueDiscoveryService : IHueDiscoveryService
{
    private readonly string _apiBaseUrl;
    private readonly string _discoveryUrl;
    private readonly HttpClient _httpClient;

    public HueDiscoveryService(HttpClient? httpClient = null, string? discoveryUrl = null, string? apiBaseUrl = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _discoveryUrl = discoveryUrl ?? "https://discovery.meethue.com/";
        _apiBaseUrl = apiBaseUrl ?? "http://";
    }

    public async Task<string?> DiscoverBridgeAsync()
    {
        try
        {
            using var resp = await _httpClient.GetAsync(_discoveryUrl);
            resp.EnsureSuccessStatusCode();
            var content = await resp.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
                return null;

            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
                return null;

            foreach (var el in doc.RootElement.EnumerateArray())
            {
                if (el.TryGetProperty("internalipaddress", out var ipProp) && ipProp.ValueKind == JsonValueKind.String)
                {
                    var ip = ipProp.GetString();
                    if (!string.IsNullOrEmpty(ip))
                        return ip;
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public Task<RegisterEntertainmentResult?> Register(string bridgeIp, string hueUser)
    {
        throw new NotImplementedException();
    }

    public Task SetColorOfLamp(Light light, RGBColor color)
    {
        throw new NotImplementedException();
    }

    public Task PulsateAsync(Light light, RGBColor color, int cycles = 3, int periodMs = 1000, int steps = 20, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Dictionary<Guid, string>> GetAllLights()
    {
        throw new NotImplementedException();
    }

    public Task<LightSnapshot> CaptureLightSnapshotAsync(Guid lightId)
    {
        throw new NotImplementedException();
    }

    public Task RestoreLightSnapshotAsync(LightSnapshot snapshot, int transitionMs = 0)
    {
        throw new NotImplementedException();
    }

    public async Task<string?> AuthenticateAsync(string bridgeIp, string deviceType)
    {
        try
        {
            var url = $"{_apiBaseUrl}{bridgeIp}/api";
            var payload = JsonSerializer.Serialize(new { devicetype = deviceType });
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
            using var resp = await _httpClient.PostAsync(url, content);
            resp.EnsureSuccessStatusCode();
            var body = await resp.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(body))
                return null;

            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
                return null;

            foreach (var el in doc.RootElement.EnumerateArray())
            {
                if (el.TryGetProperty("success", out var successProp) && successProp.ValueKind == JsonValueKind.Object)
                {
                    if (successProp.TryGetProperty("username", out var userProp) && userProp.ValueKind == JsonValueKind.String)
                        return userProp.GetString();
                }

                if (el.TryGetProperty("error", out var errorProp))
                {
                    return null;
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }


}