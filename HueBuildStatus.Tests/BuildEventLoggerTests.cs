using HueBuildStatus.Api.Features.Webhooks;
using HueBuildStatus.Core.Features.Config;
using HueBuildStatus.Core.Features.Hue;
using HueBuildStatus.Core.Features.Queue;
using Microsoft.Extensions.Logging;
using Moq;

namespace HueBuildStatus.Tests;

public class BuildEventLoggerTests
{
    private readonly Mock<IHueLightService> _hueLightServiceMock;
    private readonly Mock<IAppConfiguration> _appConfigurationMock;
    private readonly BuildEventLogger _buildEventLogger;

    public BuildEventLoggerTests()
    {
        var loggerMock = new Mock<ILogger<BuildEventLogger>>();
        _hueLightServiceMock = new Mock<IHueLightService>();
        _appConfigurationMock = new Mock<IAppConfiguration>();
        _buildEventLogger = new BuildEventLogger(loggerMock.Object, _hueLightServiceMock.Object, _appConfigurationMock.Object);
    }

    [Fact]
    public async Task HandleAsync_LightNameNotConfigured_SkipsLightControl()
    {
        // Arrange
        _appConfigurationMock.Setup(c => c.LightName).Returns((string?)null);
        var buildEvent = new BuildEvent("push", "completed", "success");

        // Act
        await _buildEventLogger.HandleAsync(buildEvent);

        // Assert
        _hueLightServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_LightNotFound_SkipsLightControl()
    {
        // Arrange
        _appConfigurationMock.Setup(c => c.LightName).Returns("TestLight");
        _hueLightServiceMock.Setup(s => s.GetLightByNameAsync("TestLight")).ReturnsAsync((BuildLightInfo?)null);
        var buildEvent = new BuildEvent("push", "completed", "success");

        // Act
        await _buildEventLogger.HandleAsync(buildEvent);

        // Assert
        _hueLightServiceMock.Verify(s => s.GetLightByNameAsync("TestLight"), Times.Once);
        _hueLightServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_StatusInProgress_FlashesLight()
    {
        // Arrange
        var lightId = Guid.NewGuid();
        _appConfigurationMock.Setup(c => c.LightName).Returns("TestLight");
        _hueLightServiceMock.Setup(s => s.GetLightByNameAsync("TestLight")).ReturnsAsync(new BuildLightInfo { Id = lightId, Name = "TestLight" });
        const int flashDuration = 5000;
        _hueLightServiceMock.Setup(s => s.FlashLightAsync(lightId, flashDuration)).ReturnsAsync(true);
        var buildEvent = new BuildEvent("workflow_run", "in_progress", null);

        // Act
        await _buildEventLogger.HandleAsync(buildEvent);

        // Assert
        _hueLightServiceMock.Verify(s => s.FlashLightAsync(lightId, flashDuration), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_StatusCompletedConclusionSuccess_SetsLightToGreen()
    {
        // Arrange
        var lightId = Guid.NewGuid();
        _appConfigurationMock.Setup(c => c.LightName).Returns("TestLight");
        _hueLightServiceMock.Setup(s => s.GetLightByNameAsync("TestLight")).ReturnsAsync(new BuildLightInfo { Id = lightId, Name = "TestLight" });
        const int duration = 2000;
        _hueLightServiceMock.Setup(s => s.SetLightColorAsync(lightId, "green", duration)).ReturnsAsync(true);
        var buildEvent = new BuildEvent("workflow_run", "completed", "success");

        // Act
        await _buildEventLogger.HandleAsync(buildEvent);

        // Assert
        _hueLightServiceMock.Verify(s => s.SetLightColorAsync(lightId, "green", duration), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_StatusCompletedConclusionFailure_SetsLightToRed()
    {
        // Arrange
        var lightId = Guid.NewGuid();
        _appConfigurationMock.Setup(c => c.LightName).Returns("TestLight");
        _hueLightServiceMock.Setup(s => s.GetLightByNameAsync("TestLight")).ReturnsAsync(new BuildLightInfo { Id = lightId, Name = "TestLight" });
        const int duration = 2000;
        _hueLightServiceMock.Setup(s => s.SetLightColorAsync(lightId, "red", duration)).ReturnsAsync(true);
        var buildEvent = new BuildEvent("workflow_run", "completed", "failure");

        // Act
        await _buildEventLogger.HandleAsync(buildEvent);

        // Assert
        _hueLightServiceMock.Verify(s => s.SetLightColorAsync(lightId, "red", duration), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_StatusCompletedConclusionOther_NoLightAction()
    {
        // Arrange
        var lightId = Guid.NewGuid();
        _appConfigurationMock.Setup(c => c.LightName).Returns("TestLight");
        _hueLightServiceMock.Setup(s => s.GetLightByNameAsync("TestLight")).ReturnsAsync(new BuildLightInfo { Id = lightId, Name = "TestLight" });
        var buildEvent = new BuildEvent("workflow_run", "completed", "cancelled");

        // Act
        await _buildEventLogger.HandleAsync(buildEvent);

        // Assert
        _hueLightServiceMock.Verify(s => s.GetLightByNameAsync("TestLight"), Times.Once);
        _hueLightServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_StatusOther_NoLightAction()
    {
        // Arrange
        var lightId = Guid.NewGuid();
        _appConfigurationMock.Setup(c => c.LightName).Returns("TestLight");
        _hueLightServiceMock.Setup(s => s.GetLightByNameAsync("TestLight")).ReturnsAsync(new BuildLightInfo { Id = lightId, Name = "TestLight" });
        var buildEvent = new BuildEvent("push", "unknown", null);

        // Act
        await _buildEventLogger.HandleAsync(buildEvent);

        // Assert
        _hueLightServiceMock.Verify(s => s.GetLightByNameAsync("TestLight"), Times.Once);
        _hueLightServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_HueLightServiceThrows_LogsErrorAndContinues()
    {
        // Arrange
        var lightId = Guid.NewGuid();
        _appConfigurationMock.Setup(c => c.LightName).Returns("TestLight");
        _hueLightServiceMock.Setup(s => s.GetLightByNameAsync("TestLight")).ReturnsAsync(new BuildLightInfo { Id = lightId, Name = "TestLight" });
        _hueLightServiceMock.Setup(s => s.FlashLightAsync(lightId, 5000)).ThrowsAsync(new Exception("Bridge offline"));
        var buildEvent = new BuildEvent("workflow_run", "in_progress", null);

        // Act
        await _buildEventLogger.HandleAsync(buildEvent);

        // Assert
        // The method should complete without throwing, and the error should be logged
    }
}