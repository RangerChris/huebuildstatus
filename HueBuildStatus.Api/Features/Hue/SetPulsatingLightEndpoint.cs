using FastEndpoints;
using HueApi.ColorConverters;
using HueApi.Models;
using HueBuildStatus.Core.Features.Hue;

namespace HueBuildStatus.Api.Features.Hue;

public class SetPulsatingLightRequest
{
    public RGBColor? Color { get; set; }
    public Light? Light { get; set; }
}

public class SetPulsatingLightEndpoint(IHueLightService hue) : Endpoint<SetPulsatingLightRequest>
{
    private readonly IHueLightService _hue = hue;

    public override void Configure()
    {
        Post("/hue/SetPulsatingLight");
        Description(s => s.WithSummary("Set Hue light to pulsate").WithDescription(""));
    }

    public override async Task HandleAsync(SetPulsatingLightRequest req, CancellationToken ct)
    {
        await Send.OkAsync(cancellation: ct);
    }
}