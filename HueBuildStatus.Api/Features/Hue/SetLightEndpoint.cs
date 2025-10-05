using FastEndpoints;
using HueBuildStatus.Core.Features.Hue;

namespace HueBuildStatus.Api.Features.Hue;

public class SetLightRequest
{
    public Guid LightId { get; set; }
    public string? ColorName { get; set; }
    public int DurationMs { get; set; } = 2000;
}

public class SetLightEndpoint(IHueLightService hue) : Endpoint<SetLightRequest>
{
    private readonly IHueLightService _hue = hue;

    public override void Configure()
    {
        Post("/hue/SetLight");
        AllowAnonymous();
        Description(s => s.WithSummary("Set Hue light color for a short duration").WithDescription("Shows a color on the light and restores the previous state."));
    }

    public override async Task HandleAsync(SetLightRequest req, CancellationToken ct)
    {
        if (req.LightId == Guid.Empty || string.IsNullOrWhiteSpace(req.ColorName))
        {
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }

        var ok = await _hue.SetLightColorAsync(req.LightId, req.ColorName!, req.DurationMs);
        if (!ok)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(cancellation: ct);
    }
}