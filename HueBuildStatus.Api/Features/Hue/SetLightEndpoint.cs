using FastEndpoints;
using HueBuildStatus.Core.Features.Hue;

namespace HueBuildStatus.Api.Features.Hue;

public class SetLightRequest
{
    public Guid LightId { get; init; }
    public string? ColorName { get; init; }
    public int DurationMs { get; init; } = 2000;
}

public class SetLightEndpoint(IHueLightService hue) : Endpoint<SetLightRequest>
{
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

        var ok = await hue.SetLightColorAsync(req.LightId, req.ColorName!, req.DurationMs);
        if (!ok)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(cancellation: ct);
    }
}