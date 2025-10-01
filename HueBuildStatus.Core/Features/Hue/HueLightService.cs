
namespace HueBuildStatus.Core.Features.Hue;

public class HueLightService : IHueLightService
{
    private readonly IHueDiscoveryService _discoveryService;
    private readonly HttpClient _httpClient;

    public HueLightService(IHueDiscoveryService discoveryService, HttpClient? httpClient = null)
    {
        _discoveryService = discoveryService ?? throw new ArgumentNullException(nameof(discoveryService));
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<string?> GetBridgeIpAsync(string? configuredBridgeIp = null)
    {
        if (!string.IsNullOrWhiteSpace(configuredBridgeIp))
            return configuredBridgeIp;

        return await _discoveryService.DiscoverBridgeAsync();
    }
}