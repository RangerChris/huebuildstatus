using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

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
            var response = await _httpClient.GetStringAsync(_discoveryUrl);
            var bridges = JsonSerializer.Deserialize<List<HueBridgeInfo>>(response);
            return bridges?.FirstOrDefault()?.InternalIpAddress;
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> AuthenticateAsync(string bridgeIp, string deviceType)
    {
        var url = $"{_apiBaseUrl}{bridgeIp}/api";
        var payload = new { devicetype = deviceType };

        try
        {
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<HueAuthResponse>>(json);
            var success = result?.FirstOrDefault(r => r.Success != null)?.Success;
            return success?.Username;
        }
        catch
        {
            return null;
        }
    }

    private class HueAuthResponse
    {
        [JsonPropertyName("success")] public HueAuthSuccess? Success { get; set; }
        [JsonPropertyName("error")] public HueAuthError? Error { get; set; }
    }

    private class HueAuthSuccess
    {
        [JsonPropertyName("username")] public string Username { get; set; } = string.Empty;
        [JsonPropertyName("clientkey")] public string? ClientKey { get; set; }
    }

    private class HueAuthError
    {
        [JsonPropertyName("type")] public int Type { get; set; }
        [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
    }

    private class HueBridgeInfo
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;

        [JsonPropertyName("internalipaddress")]
        public string InternalIpAddress { get; set; } = string.Empty;
    }
}