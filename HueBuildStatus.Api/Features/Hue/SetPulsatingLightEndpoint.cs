using FastEndpoints;
using HueBuildStatus.Core.Features.Hue;

namespace HueBuildStatus.Api.Features.Hue;

public class SetPulsatingLightRequest
{
    public Guid LightId { get; set; }
    public int DurationMs { get; set; } = 5000;
}

public class SetPulsatingLightEndpoint(IHueLightService hue) : Endpoint<SetPulsatingLightRequest>
{
    private readonly IHueLightService _hue = hue;

    public override void Configure()
    {
        Post("/hue/SetPulsatingLight");
        AllowAnonymous();
        Description(s => s.WithSummary("Flash/ pulsate a light").WithDescription("Flashes the light on/off/on/off for the specified duration and restores state."));
    }

    public override async Task HandleAsync(SetPulsatingLightRequest req, CancellationToken ct)
    {
        if (req.LightId == Guid.Empty)
        {
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }

        var ok = await _hue.FlashLightAsync(req.LightId, req.DurationMs);
        if (!ok)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(cancellation: ct);
    }
}