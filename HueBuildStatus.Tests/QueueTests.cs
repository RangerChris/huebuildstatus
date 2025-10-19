using HueBuildStatus.Core.Features.Queue;
using Moq;

namespace HueBuildStatus.Tests;

public class QueueTests
{
    [Fact]
    public async Task EnqueueAsync_ShouldNotifySubscribedHandlers()
    {
        // Arrange
        var queue = new EventQueue();
        var handlerMock = new Mock<IBuildEventHandler>();
        queue.Subscribe(handlerMock.Object);
        var buildEvent = new BuildEvent("push", "success", "success");

        // Act
        await queue.EnqueueAsync(buildEvent);

        // Assert
        handlerMock.Verify(h => h.HandleAsync(buildEvent), Times.Once);
    }

    [Fact]
    public async Task EnqueueAsync_ShouldNotifySubscribedHandlers_MultipleTimes()
    {
        // Arrange
        var queue = new EventQueue();
        var handlerMock = new Mock<IBuildEventHandler>();
        queue.Subscribe(handlerMock.Object);
        var buildEvent1 = new BuildEvent("push", "success", "success");
        var buildEvent2 = new BuildEvent("push", "success", "success");
        var buildEvent3 = new BuildEvent("push", "success", "success");

        // Act
        await queue.EnqueueAsync(buildEvent1);
        await queue.EnqueueAsync(buildEvent2);
        await queue.EnqueueAsync(buildEvent3);

        // Assert
        handlerMock.Verify(h => h.HandleAsync(buildEvent1), Times.Exactly(3));
    }

    [Fact]
    public async Task EnqueueAsync_ShouldNotifyMultipleHandlers()
    {
        // Arrange
        var queue = new EventQueue();
        var handlerMock1 = new Mock<IBuildEventHandler>();
        var handlerMock2 = new Mock<IBuildEventHandler>();
        queue.Subscribe(handlerMock1.Object);
        queue.Subscribe(handlerMock2.Object);
        var buildEvent = new BuildEvent("push", "success", "success");

        // Act
        await queue.EnqueueAsync(buildEvent);

        // Assert
        handlerMock1.Verify(h => h.HandleAsync(buildEvent), Times.Once);
        handlerMock2.Verify(h => h.HandleAsync(buildEvent), Times.Once);
    }

    [Fact]
    public async Task Unsubscribe_ShouldNotNotifyUnsubscribedHandler()
    {
        // Arrange
        var queue = new EventQueue();
        var handlerMock = new Mock<IBuildEventHandler>();
        queue.Subscribe(handlerMock.Object);
        queue.Unsubscribe(handlerMock.Object);
        var buildEvent = new BuildEvent("push", "success", "success");

        // Act
        await queue.EnqueueAsync(buildEvent);

        // Assert
        handlerMock.Verify(h => h.HandleAsync(It.IsAny<BuildEvent>()), Times.Never);
    }
}