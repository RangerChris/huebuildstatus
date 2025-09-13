using System.Text;
using System.Text.Json;

namespace HueBuildStatus.Core;

public class HueLightService(HttpClient? httpClient = null) : IHueLightService
{
    private readonly HttpClient _httpClient = httpClient ?? new HttpClient();

    public async Task<bool> SetLightOnOffAsync(string bridgeIp, string appKey, string lightId, bool on)
    {
        var url = $"http://{bridgeIp}/api/v2/resource/light/{lightId}";
        var payload = new
        {
            on = new { on }
        };
        var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        request.Headers.Add("hue-application-key", appKey);
        try
        {
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SetLightColorAsync(string bridgeIp, string appKey, string lightId, string colorHex)
    {
        // Convert hex to XY color (Hue API expects CIE xy)
        var xy = ColorConverter.HexToCIE(colorHex);
        var url = $"http://{bridgeIp}/api/v2/resource/light/{lightId}";
        var payload = new
        {
            color = new { xy = new[] { xy.x, xy.y } }
        };
        var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        request.Headers.Add("hue-application-key", appKey);
        try
        {
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SetLightBrightnessAsync(string bridgeIp, string appKey, string lightId, int brightness)
    {
        // Brightness is 0-100, Hue API expects 0-254
        var apiBrightness = Math.Clamp((int)(brightness * 2.54), 0, 254);
        var url = $"http://{bridgeIp}/api/v2/resource/light/{lightId}";
        var payload = new
        {
            dimming = new { brightness = apiBrightness }
        };
        var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        request.Headers.Add("hue-application-key", appKey);
        try
        {
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}