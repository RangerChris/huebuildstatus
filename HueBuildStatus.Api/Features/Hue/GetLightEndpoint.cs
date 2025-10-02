using FastEndpoints;
using HueApi.ColorConverters;
using HueApi.Models;
using HueBuildStatus.Core.Features.Hue;

namespace HueBuildStatus.Api.Features.Hue;

public class GetLightRequest
{
    public string? lightName { get; init; }
}

public class GetLightResponse
{
    public Light? light { get; init; }
}

public class GetLightEndpoint(IHueLightService hue) : Endpoint<GetLightRequest, GetLightResponse>
{
    private readonly IHueLightService _hue = hue;

    public override void Configure()
    {
        Get("/hue/getLight");
        Description(s => s.WithSummary("Get Hue light by name").WithDescription(""));
    }

    public override async Task HandleAsync(GetLightRequest req, CancellationToken ct)
    {
        await Send.OkAsync(new GetLightResponse()
        {
            light = new Light()
        }, cancellation: ct);
    }
}