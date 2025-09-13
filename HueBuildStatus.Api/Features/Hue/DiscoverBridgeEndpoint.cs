using FastEndpoints;
using HueBuildStatus.Core.Features.Hue;

namespace HueBuildStatus.Api.Features.Hue;

public class DiscoverBridgeEndpoint : EndpointWithoutRequest<string>
{
    private readonly IHueDiscoveryService _discovery;

    public DiscoverBridgeEndpoint(IHueDiscoveryService discovery)
    {
        _discovery = discovery;
    }

    public override void Configure()
    {
        Get("/hue/discover");
        AllowAnonymous();
        Description(x => x.WithSummary("Discover Hue bridge").WithDescription("Returns the discovered bridge IP address or 404 if not found."));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var ip = await _discovery.DiscoverBridgeAsync();
        if (string.IsNullOrEmpty(ip))
        {
            await Send.NotFoundAsync(ct);
        }
        else
        {
            await Send.OkAsync(ip, ct);
        }
    }
}