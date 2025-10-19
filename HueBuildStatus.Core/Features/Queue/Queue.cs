namespace HueBuildStatus.Core.Features.Queue;

public record BuildEvent(string GithubAction, string Status, string? Conclusion);

public interface IBuildEventHandler
{
    Task HandleAsync(BuildEvent @event);
}

public class EventQueue
{
    private readonly List<IBuildEventHandler> _handlers = [];
    private readonly object _lock = new();

    public void Subscribe(IBuildEventHandler handler)
    {
        lock (_lock)
        {
            _handlers.Add(handler);
        }
    }

    public void Unsubscribe(IBuildEventHandler handler)
    {
        lock (_lock)
        {
            _handlers.Remove(handler);
        }
    }

    public async Task EnqueueAsync(BuildEvent @event)
    {
        List<IBuildEventHandler> handlersCopy;
        lock (_lock)
        {
            handlersCopy = new List<IBuildEventHandler>(_handlers);
        }

        foreach (var handler in handlersCopy)
        {
            await handler.HandleAsync(@event);
        }
    }
}