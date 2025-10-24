using System.Diagnostics;
using FastEndpoints;
using HueBuildStatus.Core;
using HueBuildStatus.Core.Features.Config;

namespace HueBuildStatus.Api.Features.Health;

public class HealthEndpoint(IAppConfiguration config) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/health");
        AllowAnonymous();
        Description(x => x
            .WithSummary("Health check endpoint")
            .WithDescription("Returns 200 OK if the service is running and Hue bridge configuration is present.")
            .Produces(200));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var activity = new ActivitySource(TracingConstants.ActivitySourceName).StartActivity(nameof(HealthEndpoint));
        try
        {
            var bridgeIp = config.BridgeIp;
            var bridgeKey = config.BridgeKey;

            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(bridgeIp))
            {
                missing.Add("bridgeIp");
            }

            if (string.IsNullOrWhiteSpace(bridgeKey))
            {
                missing.Add("bridgeKey");
            }

            if (string.IsNullOrWhiteSpace(config.LightName))
            {
                missing.Add("LightName");
            }

            if (missing.Count == 0)
            {
                await Send.OkAsync(new { status = "Healthy" }, ct);
                return;
            }

            HttpContext.Response.StatusCode = 503;
            await HttpContext.Response.WriteAsJsonAsync(new { status = "Unhealthy", missing }, ct);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 503;
            await HttpContext.Response.WriteAsJsonAsync(new { status = "Unhealthy", error = ex.Message }, ct);
        }
        activity?.Stop();
    }
}