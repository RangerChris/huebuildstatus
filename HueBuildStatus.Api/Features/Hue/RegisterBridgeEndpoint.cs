using FastEndpoints;
using HueBuildStatus.Core.Features.Hue;

namespace HueBuildStatus.Api.Features.Hue;

public class RegisterBridgeRequest
{
    public string? Ip { get; set; }
    public string? Key { get; set; }
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
        if (string.IsNullOrWhiteSpace(req.Ip) || string.IsNullOrWhiteSpace(req.Key))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

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