using System.Diagnostics;
using HueApi.ColorConverters;
using HueBuildStatus.Core.Features.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HueBuildStatus.Core.Features.Hue;

public class HueLightService(IHueDiscoveryService discoveryService, IAppConfiguration? config = null, ILogger<HueLightService>? logger = null) : IHueLightService
{
    private readonly IHueDiscoveryService _discoveryService = discoveryService ?? throw new ArgumentNullException(nameof(discoveryService));
    private readonly ILogger<HueLightService> _logger = logger ?? NullLogger<HueLightService>.Instance;
    private static readonly ActivitySource ActivitySource = new(TracingConstants.ActivitySourceName);

    public async Task<string?> GetBridgeIpAsync(string? configuredBridgeIp = null)
    {
        var activity = ActivitySource.StartActivity(nameof(GetBridgeIpAsync));
        if (!string.IsNullOrWhiteSpace(configuredBridgeIp))
        {
            _logger.LogInformation("Using provided bridge IP {Ip}", configuredBridgeIp);
            return configuredBridgeIp;
        }

        if (!string.IsNullOrWhiteSpace(config?.BridgeIp))
        {
            _logger.LogInformation("Using configured bridge IP {Ip}", config!.BridgeIp);
            return config!.BridgeIp;
        }

        var discovered = await _discoveryService.DiscoverBridgeAsync();
        if (discovered != null)
        {
            _logger.LogInformation("Discovered bridge IP {Ip}", discovered);
        }
        else
        {
            _logger.LogWarning("Failed to discover bridge IP");
        }
        activity?.Stop();
        return discovered;
    }

    public async Task<string?> RegisterBridgeAsync(string bridgeIp)
    {
        var activity = ActivitySource.StartActivity(nameof(RegisterBridgeAsync));
        if (!string.IsNullOrWhiteSpace(config?.BridgeKey))
        {
            _logger.LogInformation("Using configured bridge key");
            return config!.BridgeKey;
        }

        if (string.IsNullOrWhiteSpace(bridgeIp))
        {
            _logger.LogWarning("Bridge IP is required for authentication");
            return null;
        }

        _logger.LogInformation("Authenticating with bridge at {Ip}", bridgeIp);
        var deviceType = "huebuildstatus#app";
        var key = await _discoveryService.AuthenticateAsync(bridgeIp, deviceType);
        if (key != null)
        {
            _logger.LogInformation("Successfully authenticated with bridge at {Ip}", bridgeIp);
        }
        else
        {
            _logger.LogWarning("Failed to authenticate with bridge at {Ip}", bridgeIp);
        }
        activity?.Stop();
        return key;
    }

    public async Task<Dictionary<Guid, string>> GetAllLightsAsync()
    {
        var activity = ActivitySource.StartActivity(nameof(GetAllLightsAsync));

        var lights = await _discoveryService.GetAllLights();
        if (lights is null)
        {
            _logger.LogWarning("Failed to retrieve lights from bridge");
            return new Dictionary<Guid, string>();
        }

        _logger.LogInformation("Retrieved {Count} lights from bridge", lights.Count);
        activity?.Stop();
        return lights;
    }

    public async Task<BuildLightInfo?> GetLightByNameAsync(string name)
    {
        var activity = ActivitySource.StartActivity(nameof(GetLightByNameAsync));
        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogWarning("Light name is required");
            return null;
        }

        var lights = await _discoveryService.GetAllLights();
        if (lights is null || lights.Count == 0)
        {
            _logger.LogWarning("No lights available on bridge");
            return null;
        }

        foreach (var kv in lights)
        {
            if (string.Equals(kv.Value, name, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Light '{Name}' found with ID {Id}", kv.Value, kv.Key);
                return new BuildLightInfo { Id = kv.Key, Name = kv.Value };
            }
        }

        _logger.LogWarning("Light '{Name}' not found", name);
        activity?.Stop();
        return null;
    }

    public async Task<LightSnapshot?> CaptureLightSnapshotAsync(Guid lightId)
    {
        var activity = ActivitySource.StartActivity(nameof(CaptureLightSnapshotAsync));
        if (lightId == Guid.Empty)
        {
            _logger.LogWarning("Invalid light ID for snapshot");
            return null;
        }

        var lights = await _discoveryService.GetAllLights();
        if (lights is null || lights.Count == 0)
        {
            _logger.LogWarning("No lights available for snapshot");
            return null;
        }

        if (!lights.ContainsKey(lightId))
        {
            _logger.LogWarning("Light {Id} not found for snapshot", lightId);
            return null;
        }

        var snapshot = await _discoveryService.CaptureLightSnapshotAsync(lightId);
        if (snapshot != null)
        {
            _logger.LogInformation("Captured snapshot for light {Id}", lightId);
        }
        else
        {
            _logger.LogWarning("Failed to capture snapshot for light {Id}", lightId);
        }
        activity?.Stop();
        return snapshot;
    }

    public async Task<bool> SetLightColorAsync(Guid lightId, string colorName, int showDurationMs = 2000)
    {
        var activity = ActivitySource.StartActivity(nameof(SetLightColorAsync));
        if (lightId == Guid.Empty)
        {
            return false;
        }

        var lights = await _discoveryService.GetAllLights();
        if (lights is null || lights.Count == 0)
        {
            return false;
        }

        if (!lights.ContainsKey(lightId))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(colorName))
        {
            return false;
        }

        _logger.LogInformation("Setting light {Id} to color {Color}", lightId, colorName);

        RGBColor color;
        switch (colorName.Trim().ToLowerInvariant())
        {
            case "red":
                color = new RGBColor(255, 0, 0);
                break;
            case "green":
                color = new RGBColor(0, 255, 0);
                break;
            case "yellow":
                color = new RGBColor(255, 255, 0);
                break;
            default:
                return false;
        }

        LightSnapshot? snapshot = null;
        try
        {
            snapshot = await _discoveryService.CaptureLightSnapshotAsync(lightId);
            await _discoveryService.SetColorOfLamp(lightId, color);
            if (showDurationMs > 0)
            {
                await Task.Delay(showDurationMs);
            }

            if (snapshot is not null)
            {
                await _discoveryService.RestoreLightSnapshotAsync(snapshot);
            }

            _logger.LogInformation("Light {Id} set to {Color}", lightId, colorName);
            return true;
        }
        catch
        {
            _logger.LogError("Failed to set light {Id} to color {Color}", lightId, colorName);
            try
            {
                if (snapshot is not null)
                {
                    await _discoveryService.RestoreLightSnapshotAsync(snapshot);
                }
            }
            catch
            {
                // swallow
            }

            activity?.Stop();
            return false;
        }
    }

    public async Task<bool> FlashLightAsync(Guid lightId, int durationMs = 5000)
    {
        var activity = ActivitySource.StartActivity(nameof(FlashLightAsync));
        if (lightId == Guid.Empty)
        {
            return false;
        }

        var lights = await _discoveryService.GetAllLights();
        if (lights is null || lights.Count == 0)
        {
            return false;
        }

        if (!lights.ContainsKey(lightId))
        {
            return false;
        }

        _logger.LogInformation("Flashing light {Id}", lightId);

        LightSnapshot? snapshot = null;
        try
        {
            snapshot = await _discoveryService.CaptureLightSnapshotAsync(lightId);

            // Perform 4 toggles: on, off, on, off over the requested duration
            var toggleCount = 4;
            var interval = durationMs > 0 ? Math.Max(1, durationMs / toggleCount) : 0;

            // on
            await _discoveryService.SetOnState(lightId, true);
            if (interval > 0)
            {
                await Task.Delay(interval);
            }

            // off
            await _discoveryService.SetOnState(lightId, false);
            if (interval > 0)
            {
                await Task.Delay(interval);
            }

            // on
            await _discoveryService.SetOnState(lightId, true);
            if (interval > 0)
            {
                await Task.Delay(interval);
            }

            // off
            await _discoveryService.SetOnState(lightId, false);
            if (interval > 0)
            {
                await Task.Delay(interval);
            }

            if (snapshot is not null)
            {
                await _discoveryService.RestoreLightSnapshotAsync(snapshot);
            }

            _logger.LogInformation("Flashed light {Id}", lightId);
            return true;
        }
        catch
        {
            _logger.LogError("Failed to flash light {Id}", lightId);
            try
            {
                if (snapshot is not null)
                {
                    await _discoveryService.RestoreLightSnapshotAsync(snapshot);
                }
            }
            catch
            {
                // swallow
            }

            activity?.Stop();
            return false;
        }
    }
}