using System.Text.Json;
using System.Text.Json.Serialization;

namespace HueBuildStatus.Core;

public class HueDiscoveryService : IHueDiscoveryService
{
    private readonly HttpClient _httpClient;

    public HueDiscoveryService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<string?> DiscoverBridgeAsync()
    {
        // Philips Hue recommends using https://discovery.meethue.com/ for bridge discovery
        var url = "https://discovery.meethue.com/";
        try
        {
            var response = await _httpClient.GetStringAsync(url);
            var bridges = JsonSerializer.Deserialize<List<HueBridgeInfo>>(response);
            return bridges?.FirstOrDefault()?.InternalIpAddress;
        }
        catch
        {
            return null;
        }
    }

    private class HueBridgeInfo
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;

        [JsonPropertyName("internalipaddress")]
        public string InternalIpAddress { get; set; } = string.Empty;
    }
}
