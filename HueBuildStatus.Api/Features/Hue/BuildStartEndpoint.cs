using FastEndpoints;
using HueBuildStatus.Core.Features.Hue;

namespace HueBuildStatus.Api.Features.Hue;

public class BuildStartRequest
{
    public Guid LightId { get; set; }
    public int DurationMs { get; set; } = 5000;
}

public class BuildStartEndpoint(IHueLightService hue) : Endpoint<BuildStartRequest>
{
    private readonly IHueLightService _hue = hue;

    public override void Configure()
    {
        Post("/hue/BuildStart");
        AllowAnonymous();
        Description(s => s.WithSummary("Start build - pulsate a light").WithDescription("Flashes/pulsates the light for the specified duration and restores state."));
    }

    public override async Task HandleAsync(BuildStartRequest req, CancellationToken ct)
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