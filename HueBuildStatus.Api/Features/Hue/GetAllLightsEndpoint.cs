using FastEndpoints;
using HueBuildStatus.Core.Features.Hue;

namespace HueBuildStatus.Api.Features.Hue;

public class GetAllLightsEndpoint(IHueLightService lightService) : EndpointWithoutRequest<AllLightsResponse>
{
    public override void Configure()
    {
        Get("/hue/GetAllLights");
        AllowAnonymous();
        Description(x => x.WithSummary("Returns a list of all lights found").WithDescription(""));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var dict = await lightService.GetAllLightsAsync();
        var resp = new AllLightsResponse { NameList = dict ?? new Dictionary<Guid, string>() };
        await Send.OkAsync(resp, ct);
    }
}

public class AllLightsResponse
{
    public Dictionary<Guid, string> NameList { get; set; } = new();
}