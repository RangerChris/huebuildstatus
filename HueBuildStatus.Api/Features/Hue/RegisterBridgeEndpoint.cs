using FastEndpoints;
using HueBuildStatus.Core.Features.Hue;

namespace HueBuildStatus.Api.Features.Hue;

public class RegisterBridgeRequest
{
    public string? Ip { get; init; }
    public string? Key { get; init; }
}

public class RegisterBridgeEndpoint(IHueLightService lightService) : Endpoint<RegisterBridgeRequest, string?>
{
    public override void Configure()
    {
        Post("/hue/register");
        AllowAnonymous();
        Description(x => x.WithSummary("Register the Hue bridge").WithDescription("Registers the Hue bridge and returns an app key."));
    }

    public override async Task HandleAsync(RegisterBridgeRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Ip))
        {
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }

        var result = await lightService.RegisterBridgeAsync(req.Ip, req.Key);
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