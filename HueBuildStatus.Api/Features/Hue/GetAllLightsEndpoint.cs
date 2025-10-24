using System.Diagnostics;
using FastEndpoints;
using HueBuildStatus.Core;
using HueBuildStatus.Core.Features.Hue;

namespace HueBuildStatus.Api.Features.Hue;

public class GetAllLightsEndpoint(IHueLightService lightService) : EndpointWithoutRequest<AllLightsResponse>
{
    public override void Configure()
    {
        Get("/hue/GetAllLights");
        AllowAnonymous();
        Description(x => x
            .WithSummary("Returns a list of all lights found")
            .WithDescription("Retrieves all lights connected to the configured Hue bridge. Requires bridgeIp and bridgeKey to be set in appsettings.json.")
            .Produces(200));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var activity = new ActivitySource(TracingConstants.ActivitySourceName).StartActivity(nameof(GetAllLightsEndpoint));
        var dict = await lightService.GetAllLightsAsync();
        var resp = new AllLightsResponse { NameList = dict ?? new Dictionary<Guid, string>() };
        await Send.OkAsync(resp, ct);
        activity?.Stop();
    }
}

public class AllLightsResponse
{
    public Dictionary<Guid, string> NameList { get; set; } = new();
}