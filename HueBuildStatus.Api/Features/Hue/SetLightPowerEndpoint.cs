using FastEndpoints;
using HueBuildStatus.Core.Features.Hue;

namespace HueBuildStatus.Api.Features.Hue;

public class SetLightPowerRequest
{
    public string BridgeIp { get; set; } = string.Empty;
    public string AppKey { get; set; } = string.Empty;
    public string LightId { get; set; } = string.Empty;
    public bool On { get; set; }
}

public class SetLightPowerEndpoint : Endpoint<SetLightPowerRequest>
{
    private readonly IHueLightService _hue;

    public SetLightPowerEndpoint(IHueLightService hue)
    {
        _hue = hue;
    }

    public override void Configure()
    {
        Post("/hue/light/power");
        Description(s => s.WithSummary("Set Hue light power").WithDescription("Turn a Hue light on or off."));
    }

    public override async Task HandleAsync(SetLightPowerRequest req, CancellationToken ct)
    {
        var ok = await _hue.SetLightOnOffAsync(req.BridgeIp, req.AppKey, req.LightId, req.On);
        if (!ok)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsync("Failed to set light power", ct);
            return;
        }

        await Send.OkAsync(cancellation: ct);
    }
}