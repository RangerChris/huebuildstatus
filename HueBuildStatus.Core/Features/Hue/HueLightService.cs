using System.Text;
using System.Text.Json;

namespace HueBuildStatus.Core.Features.Hue;

public class HueLightService(HttpClient? httpClient = null) : IHueLightService
{
    private readonly HttpClient _httpClient = httpClient ?? new HttpClient();
}