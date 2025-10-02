using FastEndpoints;

namespace HueBuildStatus.Api.Features.Hue;

public class GetAllLightsEndpoint : EndpointWithoutRequest<AllLightsResponse>
{
    public override void Configure()
    {
        Get("/hue/GetAllLights");
        AllowAnonymous();
        Description(x => x.WithSummary("Returns a list of all lights found").WithDescription(""));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.OkAsync(new AllLightsResponse(), ct);
    }
}

public class AllLightsResponse
{
    public Dictionary<Guid, string> NameList { get; set; } = new();
}