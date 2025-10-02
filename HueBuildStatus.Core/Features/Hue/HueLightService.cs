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
}