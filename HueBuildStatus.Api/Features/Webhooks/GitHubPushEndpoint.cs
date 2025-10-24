using System.Diagnostics;
using System.Text.Json;
using FastEndpoints;
using HueBuildStatus.Core;
using HueBuildStatus.Core.Features.Queue;

namespace HueBuildStatus.Api.Features.Webhooks;

public class GitHubPushEndpoint(ILogger<GitHubPushEndpoint> logger, EventQueue queue) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/webhooks/github");
        AllowAnonymous();
        Description(s => s
            .WithSummary("Receive GitHub webhook")
            .WithDescription("Receives webhook payloads from GitHub for push events and other actions. Processes the payload to trigger build status updates. Configure webhooks in your GitHub repository settings pointing to this endpoint.")
            .Produces(200)
            .Produces(400));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var activity = new ActivitySource(TracingConstants.ActivitySourceName).StartActivity(nameof(GitHubPushEndpoint));
        string? githubEvent = HttpContext.Request.Headers["X-GitHub-Event"];
        if (string.IsNullOrWhiteSpace(githubEvent))
        {
            logger.LogInformation("Missing 'X-GitHub-Event' header");
            await Send.ErrorsAsync(400, ct);
            return;
        }

        logger.LogInformation("X-GitHub-Event: {GitHubEvent}", githubEvent);

        string responseContent;
        using (var reader = new StreamReader(HttpContext.Request.Body))
        {
            responseContent = await reader.ReadToEndAsync(ct);
        }

        if (string.IsNullOrWhiteSpace(responseContent))
        {
            logger.LogInformation("Received empty or null JSON payload");
            await Send.ErrorsAsync(400, ct);
            return;
        }

        try
        {
            using var doc = JsonDocument.Parse(responseContent);
            if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.GetRawText() == "{}")
            {
                logger.LogInformation("Received empty JSON object");
                await Send.ErrorsAsync(400, ct);
                return;
            }

            var status = JsonHelper.FindJsonProperty(doc.RootElement, "status");
            var conclusion = JsonHelper.FindJsonProperty(doc.RootElement, "conclusion");

            if (string.IsNullOrEmpty(status))
            {
                logger.LogInformation("Webhook payload missing status");
                await Send.ErrorsAsync(400, ct);
                return;
            }

            logger.LogInformation("Status: {Status}, Conclusion: {Conclusion}", status, conclusion);

            // Add status, conclusion and githubEvent to EventQueue
            await queue.EnqueueAsync(new BuildEvent(githubEvent, status ?? "", conclusion ?? ""));

            await Send.OkAsync(new { status, conclusion }, cancellation: ct);
        }
        catch (JsonException)
        {
            logger.LogInformation("Received malformed JSON payload");
            await Send.ErrorsAsync(400, ct);
        }
        activity?.Stop();
    }
}