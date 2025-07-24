using FastEndpoints;

namespace HueBuildStatus.Api;

public class GitHubPushEndpoint : Endpoint<GitHubPushEvent>
{
    public override void Configure()
    {
        Post("/webhook/github/push");
        AllowAnonymous(); // For now, allow anonymous for testing
        Description(x => x.WithSummary("Receives GitHub push event payloads."));
    }

    public override async Task HandleAsync(GitHubPushEvent req, CancellationToken ct)
    {
        // Mocked service logic: just respond with a message
        await Send.ResponseAsync($"Received push for repo: {req.Repository.Name}", cancellation: ct);
    }
}
