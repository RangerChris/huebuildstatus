using FastEndpoints;
using HueBuildStatus.Core.Features.Hue;

namespace HueBuildStatus.Api.Features.Hue;

public class BuildStartRequest
{
    public Guid LightId { get; init; }
    public int DurationMs { get; init; } = 5000;
}

public class BuildStartEndpoint(IHueLightService hue) : Endpoint<BuildStartRequest>
{
    public override void Configure()
    {
        Post("/hue/BuildStart");
        AllowAnonymous();
        Description(s => s
            .WithSummary("Start build - pulsate a light")
            .WithDescription("Flashes/pulsates the specified light (yellow color) for the given duration (default 5 seconds) to indicate a build is in progress, then restores the previous state. Requires bridgeIp and bridgeKey to be set in appsettings.json.")
            .Accepts<BuildStartRequest>("Request containing light ID and flash duration")
            .Produces(200)
            .Produces(404));
    }

    public override async Task HandleAsync(BuildStartRequest req, CancellationToken ct)
    {
        if (req.LightId == Guid.Empty)
        {
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }

        var ok = await hue.FlashLightAsync(req.LightId, req.DurationMs);
        if (!ok)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(cancellation: ct);
    }
}