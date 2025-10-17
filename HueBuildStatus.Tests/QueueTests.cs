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