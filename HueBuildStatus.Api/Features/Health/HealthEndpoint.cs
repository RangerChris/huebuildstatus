using FastEndpoints;
using HueBuildStatus.Core.Features.Config;
using Microsoft.AspNetCore.Http;

namespace HueBuildStatus.Api.Features.Health;

public class HealthEndpoint : EndpointWithoutRequest
{
    private readonly IAppConfiguration _config;

    public HealthEndpoint(IAppConfiguration config)
    {
        _config = config;
    }

    public override void Configure()
    {
        Get("/health");
        AllowAnonymous();
        Description(x => x
            .WithSummary("Health check endpoint")
            .WithDescription("Returns 200 OK if the service is running and Hue bridge configuration is present."));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var bridgeIp = _config.BridgeIp;
            var bridgeKey = _config.BridgeKey;

            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(bridgeIp)) missing.Add("bridgeIp");
            if (string.IsNullOrWhiteSpace(bridgeKey)) missing.Add("bridgeKey");

            if (missing.Count == 0)
            {
                await Send.OkAsync(new { status = "Healthy" }, cancellation: ct);
                return;
            }

            HttpContext.Response.StatusCode = 503;
            await HttpContext.Response.WriteAsJsonAsync(new { status = "Unhealthy", missing }, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 503;
            await HttpContext.Response.WriteAsJsonAsync(new { status = "Unhealthy", error = ex.Message }, cancellationToken: ct);
        }
    }
}