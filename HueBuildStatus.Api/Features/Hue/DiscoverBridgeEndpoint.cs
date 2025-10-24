using System.Diagnostics;
using FastEndpoints;
using HueBuildStatus.Core;
using HueBuildStatus.Core.Features.Hue;

namespace HueBuildStatus.Api.Features.Hue;

public class DiscoverBridgeEndpoint(IHueLightService lightService) : EndpointWithoutRequest<string>
{
    public override void Configure()
    {
        Get("/hue/discover");
        AllowAnonymous();
        Description(x => x
            .WithSummary("Discover Hue bridge")
            .WithDescription("Automatically discovers the IP address of the Hue bridge on the local network. This IP can be used to set the 'bridgeIp' in appsettings.json for subsequent operations.")
            .Produces(200)
            .Produces(404));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var activity = new ActivitySource(TracingConstants.ActivitySourceName).StartActivity(nameof(DiscoverBridgeEndpoint));
        var ip = await lightService.GetBridgeIpAsync();
        if (string.IsNullOrEmpty(ip))
        {
            await Send.NotFoundAsync(ct);
        }
        else
        {
            await Send.OkAsync(ip, ct);
        }
        activity?.Stop();
    }
}