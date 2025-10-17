using HueBuildStatus.Core.Features.Queue;

namespace HueBuildStatus.Api.Features.Webhooks;

public class BuildEventLogger(ILogger<BuildEventLogger> logger) : IBuildEventHandler
{
    public Task HandleAsync(BuildEvent @event)
    {
        logger.LogInformation("Received build event: Action={Action}, Status={Status}, Conclusion={Conclusion}",
            @event.GithubAction, @event.Status, @event.Conclusion);
        return Task.CompletedTask;
    }
}