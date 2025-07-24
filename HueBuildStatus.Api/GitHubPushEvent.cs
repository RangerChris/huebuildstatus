namespace HueBuildStatus.Api;

public class GitHubPushEvent
{
    public string Ref { get; set; } = string.Empty;
    public string Before { get; set; } = string.Empty;
    public string After { get; set; } = string.Empty;
    public GitHubRepository Repository { get; set; } = new();
    public GitHubPusher Pusher { get; set; } = new();
}

public class GitHubRepository
{
    public string Name { get; set; } = string.Empty;
}

public class GitHubPusher
{
    public string Name { get; set; } = string.Empty;
}