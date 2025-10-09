using FastEndpoints;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace HueBuildStatus.Api.Features.Webhooks;

public class GitHubWebhookPayload
{
    [JsonPropertyName("ref")]
    public string Ref { get; set; } = string.Empty;
    [JsonPropertyName("repository")]
    public RepositoryInfo Repository { get; set; } = new();
    [JsonPropertyName("pusher")]
    public PusherInfo Pusher { get; set; } = new();
    [JsonPropertyName("head_commit")]
    public HeadCommitInfo Head_Commit { get; set; } = new();
}

public class RepositoryInfo
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class PusherInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class HeadCommitInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

public class GitHubPushEndpoint(ILogger<GitHubPushEndpoint> logger) : Endpoint<GitHubWebhookPayload>
{
    public override void Configure()
    {
        Post("/webhooks/github");
        AllowAnonymous();
        Description(s => s.WithSummary("Receive GitHub webhook").WithDescription("Logs all received GitHub webhook information."));
    }

    public override async Task HandleAsync(GitHubWebhookPayload req, CancellationToken ct)
    {
        logger.LogInformation("GitHub webhook payload: {@Payload}", req);
        await Send.OkAsync(cancellation: ct);
    }
}