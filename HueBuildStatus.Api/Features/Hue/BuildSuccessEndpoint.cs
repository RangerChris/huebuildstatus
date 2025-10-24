using System.Diagnostics;
using FastEndpoints;
using HueBuildStatus.Core;
using HueBuildStatus.Core.Features.Hue;

namespace HueBuildStatus.Api.Features.Hue;

public class BuildSuccessRequest
{
    public Guid LightId { get; init; }
    public int DurationMs { get; init; } = 5000;
}

public class BuildSuccessEndpoint(IHueLightService hue) : Endpoint<BuildSuccessRequest>
{
    public override void Configure()
    {
        Post("/hue/BuildSuccess");
        AllowAnonymous();
        Description(s => s
            .WithSummary("Set light to green for build success")
            .WithDescription("Shows green on the specified light for 5 seconds to indicate a successful build, then restores the previous state. Requires bridgeIp and bridgeKey to be set in appsettings.json.")
            .Produces(200)
            .Produces(404));
    }

    public override async Task HandleAsync(BuildSuccessRequest req, CancellationToken ct)
    {
        var activity = new ActivitySource(TracingConstants.ActivitySourceName).StartActivity(nameof(BuildSuccessEndpoint));
        if (req.LightId == Guid.Empty)
        {
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }

        var ok = await hue.SetLightColorAsync(req.LightId, "green", req.DurationMs);
        if (!ok)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(cancellation: ct);
        activity?.Stop();
    }
}