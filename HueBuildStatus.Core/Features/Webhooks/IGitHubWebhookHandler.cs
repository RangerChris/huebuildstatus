// New interface for handling GitHub webhooks

namespace HueBuildStatus.Core.Features.Webhooks;

public interface IGitHubWebhookHandler
{
    Task HandlePushAsync(GitHubPushPayload payload, CancellationToken ct = default);
}