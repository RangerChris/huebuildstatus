using System.Net.Http;
using System.Text.Json;
using System.Text;

namespace HueBuildStatus.Core;

public class HueLightService : IHueLightService
{
    private readonly HttpClient _httpClient;

    public HueLightService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<bool> SetLightOnOffAsync(string bridgeIp, string appKey, string lightId, bool on)
    {
        var url = $"http://{bridgeIp}/api/v2/resource/light/{lightId}";
        var payload = new
        {
            on = new { on = on }
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
