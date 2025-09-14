// New DTO representing the minimal fields from a GitHub push event payload

namespace HueBuildStatus.Core.Features.Webhooks;

public class GitHubPushPayload
{
    public string Ref { get; set; } = string.Empty;
    public string RepositoryName { get; set; } = string.Empty;
    public string PusherName { get; set; } = string.Empty;
    public string HeadCommitId { get; set; } = string.Empty;
}