using FastEndpoints;
using HueBuildStatus.Core.Features.Hue;

namespace HueBuildStatus.Api.Features.Hue;

public class RegisterBridgeRequest
{
    public string Ip { get; set; }
    public string Key { get; set; }
}

public class RegisterBridgeEndpoint(IHueDiscoveryService discovery) : Endpoint<RegisterBridgeRequest>
{
    public override void Configure()
    {
        Get("/hue/register");
        AllowAnonymous();
        Description(x => x.WithSummary("Register the Hue bridge").WithDescription(""));
    }

    public override async Task HandleAsync(RegisterBridgeRequest req, CancellationToken ct)
    {
        var result = await discovery.Register(req.Ip, req.Key);
        if (result is null)
        {
            await Send.NotFoundAsync(ct);
        }
        else
        {
            await Send.OkAsync(result, ct);
        }
    }
}