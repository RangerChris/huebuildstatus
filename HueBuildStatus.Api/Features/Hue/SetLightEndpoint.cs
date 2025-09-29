using FastEndpoints;
using HueApi.ColorConverters;
using HueApi.Models;
using HueBuildStatus.Core.Features.Hue;

namespace HueBuildStatus.Api.Features.Hue;

public class SetLightRequest
{
    public RGBColor Color { get; set; }
    public Light Light { get; set; }
}

public class SetLightEndpoint(IHueLightService hue) : Endpoint<SetLightRequest>
{
    private readonly IHueLightService _hue = hue;

    public override void Configure()
    {
        Post("/hue/SetLight");
        Description(s => s.WithSummary("Set Hue light").WithDescription(""));
    }

    public override async Task HandleAsync(SetLightRequest req, CancellationToken ct)
    {
        await Send.OkAsync(cancellation: ct);
    }
}