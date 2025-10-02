using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HueApi.ColorConverters;

namespace HueBuildStatus.Core.Features.Hue;

public class HueLightService : IHueLightService
{
    private readonly IHueDiscoveryService _discoveryService;

    public HueLightService(IHueDiscoveryService discoveryService)
    {
        _discoveryService = discoveryService ?? throw new ArgumentNullException(nameof(discoveryService));
    }

    public async Task<string?> GetBridgeIpAsync(string? configuredBridgeIp = null)
    {
        if (!string.IsNullOrWhiteSpace(configuredBridgeIp))
        {
            return configuredBridgeIp;
        }

        return await _discoveryService.DiscoverBridgeAsync();
    }

    public async Task<string?> RegisterBridgeAsync(string bridgeIp, string? configuredBridgeKey = null)
    {
        if (!string.IsNullOrWhiteSpace(configuredBridgeKey))
        {
            return configuredBridgeKey;
        }

        if (string.IsNullOrWhiteSpace(bridgeIp))
        {
            return null;
        }

        var deviceType = "huebuildstatus#app";
        return await _discoveryService.AuthenticateAsync(bridgeIp, deviceType);
    }

    public async Task<Dictionary<Guid, string>> GetAllLightsAsync()
    {
        var lights = await _discoveryService.GetAllLights();
        if (lights is null)
        {
            return new Dictionary<Guid, string>();
        }

        return lights;
    }

    public async Task<LightInfo?> GetLightByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var lights = await _discoveryService.GetAllLights();
        if (lights is null || lights.Count == 0)
        {
            return null;
        }

        foreach (var kv in lights)
        {
            if (string.Equals(kv.Value, name, StringComparison.OrdinalIgnoreCase))
            {
                return new LightInfo { Id = kv.Key, Name = kv.Value };
            }
        }

        return null;
    }

    public async Task<LightSnapshot?> CaptureLightSnapshotAsync(Guid lightId)
    {
        if (lightId == Guid.Empty)
        {
            return null;
        }

        var lights = await _discoveryService.GetAllLights();
        if (lights is null || lights.Count == 0)
        {
            return null;
        }

        if (!lights.ContainsKey(lightId))
        {
            return null;
        }

        return await _discoveryService.CaptureLightSnapshotAsync(lightId);
    }

    public async Task<bool> SetLightColorAsync(Guid lightId, string colorName, int showDurationMs = 2000)
    {
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
                await _discoveryService.RestoreLightSnapshotAsync(snapshot, 0);
            }

            return true;
        }
        catch
        {
            try
            {
                if (snapshot is not null)
                {
                    await _discoveryService.RestoreLightSnapshotAsync(snapshot, 0);
                }
            }
            catch
            {
                // swallow
            }

            return false;
        }
    }
}