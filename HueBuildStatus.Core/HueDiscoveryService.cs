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
    public async Task<string?> AuthenticateAsync(string bridgeIp, string deviceType)
    {
        // The user must press the link button on the bridge before this request
        var url = $"http://{bridgeIp}/api";
        var payload = new { devicetype = deviceType }; // Hue API expects { "devicetype": "appname#devicename" }
        try
        {
            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            var json = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<List<HueAuthResponse>>(json);
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
        [System.Text.Json.Serialization.JsonPropertyName("success")]
        public HueAuthSuccess? Success { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("error")]
        public HueAuthError? Error { get; set; }
    }
    private class HueAuthSuccess
    {
        [System.Text.Json.Serialization.JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonPropertyName("clientkey")]
        public string? ClientKey { get; set; }
    }
    private class HueAuthError
    {
        [System.Text.Json.Serialization.JsonPropertyName("type")]
        public int Type { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }

    private class HueBridgeInfo
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;

        [JsonPropertyName("internalipaddress")]
        public string InternalIpAddress { get; set; } = string.Empty;
    }
}
