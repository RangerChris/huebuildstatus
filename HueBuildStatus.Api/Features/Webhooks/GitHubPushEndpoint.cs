using System.Text.Json;
using FastEndpoints;

namespace HueBuildStatus.Api.Features.Webhooks;

public class GitHubPushEndpoint(ILogger<GitHubPushEndpoint> logger) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/webhooks/github");
        AllowAnonymous();
        Description(s => s.WithSummary("Receive GitHub webhook").WithDescription("Logs all received GitHub webhook information."));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        string req;
        using (var reader = new StreamReader(HttpContext.Request.Body))
        {
            req = await reader.ReadToEndAsync(ct);
        }

        if (string.IsNullOrWhiteSpace(req))
        {
            logger.LogInformation("Received empty or null JSON payload");
            await Send.ErrorsAsync(400, ct);
            return;
        }

        try
        {
            using var doc = JsonDocument.Parse(req);
            if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.GetRawText() == "{}")
            {
                logger.LogInformation("Received empty JSON object");
                await Send.ErrorsAsync(400, ct);
                return;
            }
            logger.LogInformation("Deserialized payload: {Payload}", req);
            await Send.OkAsync(cancellation: ct);
        }
        catch (JsonException)
        {
            logger.LogInformation("Received malformed JSON payload");
            await Send.ErrorsAsync(400, ct);
        }
    }
}