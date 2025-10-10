using FastEndpoints;
using HueBuildStatus.Core.Features.Hue;

namespace HueBuildStatus.Api.Features.Hue;

public class BuildSuccessRequest
{
    public Guid LightId { get; init; }
}

public class BuildSuccessEndpoint(IHueLightService hue) : Endpoint<BuildSuccessRequest>
{
    public override void Configure()
    {
        Post("/hue/BuildSuccess");
        AllowAnonymous();
        Description(s => s.WithSummary("Set light to green for build success").WithDescription("Shows green on the specified light for 5 seconds and restores the previous state."));
    }

    public override async Task HandleAsync(BuildSuccessRequest req, CancellationToken ct)
    {
        if (req.LightId == Guid.Empty)
        {
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }

        var ok = await hue.SetLightColorAsync(req.LightId, "green", 5000);
        if (!ok)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(cancellation: ct);
    }
}