// New FastEndpoints endpoint to receive GitHub push webhooks

using FastEndpoints;
using HueBuildStatus.Core.Features.Webhooks;

namespace HueBuildStatus.Api.Features.Webhooks;

public class GitHubPushEndpoint : Endpoint<GitHubPushPayload>
{
    public override void Configure()
    {
        Post("/webhooks/github/push");
        AllowAnonymous();
        Description(s => s.WithSummary("Receive GitHub push webhook").WithDescription("Processes GitHub push events."));
    }

    public override async Task HandleAsync(GitHubPushPayload req, CancellationToken ct)
    {
        await Send.OkAsync(cancellation: ct);
    }
}