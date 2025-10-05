using FastEndpoints;
using HueBuildStatus.Core.Features.Hue;

namespace HueBuildStatus.Api.Features.Hue;

public class BuildFailedRequest
{
    public Guid LightId { get; set; }
}

public class BuildFailedEndpoint(IHueLightService hue) : Endpoint<BuildFailedRequest>
{
    private readonly IHueLightService _hue = hue;

    public override void Configure()
    {
        Post("/hue/BuildFailed");
        AllowAnonymous();
        Description(s => s.WithSummary("Set light to red for build failure").WithDescription("Shows red on the specified light for 5 seconds and restores the previous state."));
    }

    public override async Task HandleAsync(BuildFailedRequest req, CancellationToken ct)
    {
        if (req.LightId == Guid.Empty)
        {
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }

        var ok = await _hue.SetLightColorAsync(req.LightId, "red", 5000);
        if (!ok)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(cancellation: ct);
    }
}