using FastEndpoints;
using HueBuildStatus.Core.Features.Hue;

namespace HueBuildStatus.Api.Features.Hue;

public class GetLightRequest
{
    public string? lightName { get; init; }
}

public class GetLightEndpoint(IHueLightService hue) : Endpoint<GetLightRequest, LightInfo>
{
    private readonly IHueLightService _hue = hue;

    public override void Configure()
    {
        Get("/hue/getlight");
        AllowAnonymous();
        Description(s => s.WithSummary("Get Hue light by name").WithDescription(""));
    }

    public override async Task HandleAsync(GetLightRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.lightName))
        {
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }

        var info = await _hue.GetLightByNameAsync(req.lightName);
        if (info is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(info, ct);
    }
}