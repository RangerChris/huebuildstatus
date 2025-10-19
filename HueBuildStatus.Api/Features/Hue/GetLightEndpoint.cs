using FastEndpoints;
using HueBuildStatus.Core.Features.Hue;

namespace HueBuildStatus.Api.Features.Hue;

public class GetLightRequest
{
    public string? lightName { get; init; }
}

public class GetLightEndpoint(IHueLightService hue) : Endpoint<GetLightRequest, BuildLightInfo>
{
    public override void Configure()
    {
        Get("/hue/getlight");
        AllowAnonymous();
        Description(s => s
            .WithSummary("Get Hue light by name")
            .WithDescription("Retrieves information about a specific light by its name. Requires bridgeIp and bridgeKey to be set in appsettings.json.")
            .Produces(200)
            .Produces(404));
    }

    public override async Task HandleAsync(GetLightRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.lightName))
        {
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }

        var info = await hue.GetLightByNameAsync(req.lightName);
        if (info is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(info, ct);
    }
}