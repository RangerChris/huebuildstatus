using HueBuildStatus.Core.Features.Config;
using HueBuildStatus.Core.Features.Hue;
using Microsoft.Extensions.Logging;
using Moq;

namespace HueBuildStatus.Tests;

public class HueLightServiceTests
{
    private readonly Mock<IHueDiscoveryService> _discoveryServiceMock;
    private readonly Mock<IAppConfiguration> _configMock;
    private readonly HueLightService _hueLightService;

    public HueLightServiceTests()
    {
        _discoveryServiceMock = new Mock<IHueDiscoveryService>();
        _configMock = new Mock<IAppConfiguration>();
        var loggerMock = new Mock<ILogger<HueLightService>>();
        _hueLightService = new HueLightService(_discoveryServiceMock.Object, _configMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task GetBridgeIpAsync_ReturnsProvidedIp()
    {
        // Act
        var result = await _hueLightService.GetBridgeIpAsync("192.168.1.1");

        // Assert
        Assert.Equal("192.168.1.1", result);
    }

    [Fact]
    public async Task GetBridgeIpAsync_ReturnsConfiguredIp()
    {
        // Arrange
        _configMock.Setup(c => c.BridgeIp).Returns("192.168.1.2");

        // Act
        var result = await _hueLightService.GetBridgeIpAsync();

        // Assert
        Assert.Equal("192.168.1.2", result);
    }

    [Fact]
    public async Task GetBridgeIpAsync_DiscoversIp()
    {
        // Arrange
        _discoveryServiceMock.Setup(d => d.DiscoverBridgeAsync()).ReturnsAsync("192.168.1.3");

        // Act
        var result = await _hueLightService.GetBridgeIpAsync();

        // Assert
        Assert.Equal("192.168.1.3", result);
    }

    [Fact]
    public async Task RegisterBridgeAsync_ReturnsProvidedKey()
    {
        // Act
        var result = await _hueLightService.RegisterBridgeAsync("192.168.1.1");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RegisterBridgeAsync_ReturnsConfiguredKey()
    {
        // Arrange
        _configMock.Setup(c => c.BridgeKey).Returns("key456");

        // Act
        var result = await _hueLightService.RegisterBridgeAsync("192.168.1.1");

        // Assert
        Assert.Equal("key456", result);
    }

    [Fact]
    public async Task RegisterBridgeAsync_Authenticates()
    {
        // Arrange
        _discoveryServiceMock.Setup(d => d.AuthenticateAsync("192.168.1.1", "huebuildstatus#app")).ReturnsAsync("key789");

        // Act
        var result = await _hueLightService.RegisterBridgeAsync("192.168.1.1");

        // Assert
        Assert.Equal("key789", result);
    }

    [Fact]
    public async Task RegisterBridgeAsync_ReturnsNullForEmptyIp()
    {
        // Act
        var result = await _hueLightService.RegisterBridgeAsync("");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllLightsAsync_ReturnsLights()
    {
        // Arrange
        var lights = new Dictionary<Guid, string> { { Guid.NewGuid(), "Light1" } };
        _discoveryServiceMock.Setup(d => d.GetAllLights()).ReturnsAsync(lights);

        // Act
        var result = await _hueLightService.GetAllLightsAsync();

        // Assert
        Assert.Equal(lights, result);
    }

    [Fact]
    public async Task GetAllLightsAsync_ReturnsEmptyWhenNull()
    {
        // Arrange
        _discoveryServiceMock.Setup(d => d.GetAllLights()).ReturnsAsync((Dictionary<Guid, string>?)null);

        // Act
        var result = await _hueLightService.GetAllLightsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetLightByNameAsync_ReturnsNullForEmptyName()
    {
        // Act
        var result = await _hueLightService.GetLightByNameAsync("");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetLightByNameAsync_ReturnsNullWhenNoLights()
    {
        // Arrange
        _discoveryServiceMock.Setup(d => d.GetAllLights()).ReturnsAsync(new Dictionary<Guid, string>());

        // Act
        var result = await _hueLightService.GetLightByNameAsync("Light1");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetLightByNameAsync_ReturnsLight()
    {
        // Arrange
        var lightId = Guid.NewGuid();
        var lights = new Dictionary<Guid, string> { { lightId, "Light1" } };
        _discoveryServiceMock.Setup(d => d.GetAllLights()).ReturnsAsync(lights);

        // Act
        var result = await _hueLightService.GetLightByNameAsync("Light1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(lightId, result.Id);
        Assert.Equal("Light1", result.Name);
    }

    [Fact]
    public async Task GetLightByNameAsync_ReturnsNullWhenNotFound()
    {
        // Arrange
        var lights = new Dictionary<Guid, string> { { Guid.NewGuid(), "Light1" } };
        _discoveryServiceMock.Setup(d => d.GetAllLights()).ReturnsAsync(lights);

        // Act
        var result = await _hueLightService.GetLightByNameAsync("Light2");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CaptureLightSnapshotAsync_ReturnsNullForEmptyId()
    {
        // Act
        var result = await _hueLightService.CaptureLightSnapshotAsync(Guid.Empty);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CaptureLightSnapshotAsync_ReturnsNullWhenNoLights()
    {
        // Arrange
        _discoveryServiceMock.Setup(d => d.GetAllLights()).ReturnsAsync(new Dictionary<Guid, string>());

        // Act
        var result = await _hueLightService.CaptureLightSnapshotAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CaptureLightSnapshotAsync_ReturnsNullWhenLightNotFound()
    {
        // Arrange
        var lights = new Dictionary<Guid, string> { { Guid.NewGuid(), "Light1" } };
        _discoveryServiceMock.Setup(d => d.GetAllLights()).ReturnsAsync(lights);

        // Act
        var result = await _hueLightService.CaptureLightSnapshotAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CaptureLightSnapshotAsync_ReturnsSnapshot()
    {
        // Arrange
        var lightId = Guid.NewGuid();
        var lights = new Dictionary<Guid, string> { { lightId, "Light1" } };
        var snapshot = new LightSnapshot(lightId, "Light1");
        _discoveryServiceMock.Setup(d => d.GetAllLights()).ReturnsAsync(lights);
        _discoveryServiceMock.Setup(d => d.CaptureLightSnapshotAsync(lightId)).ReturnsAsync(snapshot);

        // Act
        var result = await _hueLightService.CaptureLightSnapshotAsync(lightId);

        // Assert
        Assert.Equal(snapshot, result);
    }

    [Fact]
    public async Task SetLightColorAsync_ReturnsFalseForEmptyId()
    {
        // Act
        var result = await _hueLightService.SetLightColorAsync(Guid.Empty, "red");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SetLightColorAsync_ReturnsFalseWhenNoLights()
    {
        // Arrange
        _discoveryServiceMock.Setup(d => d.GetAllLights()).ReturnsAsync(new Dictionary<Guid, string>());

        // Act
        var result = await _hueLightService.SetLightColorAsync(Guid.NewGuid(), "red");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SetLightColorAsync_ReturnsFalseWhenLightNotFound()
    {
        // Arrange
        var lights = new Dictionary<Guid, string> { { Guid.NewGuid(), "Light1" } };
        _discoveryServiceMock.Setup(d => d.GetAllLights()).ReturnsAsync(lights);

        // Act
        var result = await _hueLightService.SetLightColorAsync(Guid.NewGuid(), "red");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SetLightColorAsync_ReturnsFalseForInvalidColor()
    {
        // Arrange
        var lightId = Guid.NewGuid();
        var lights = new Dictionary<Guid, string> { { lightId, "Light1" } };
        _discoveryServiceMock.Setup(d => d.GetAllLights()).ReturnsAsync(lights);

        // Act
        var result = await _hueLightService.SetLightColorAsync(lightId, "invalid");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SetLightColorAsync_SetsColorSuccessfully()
    {
        // Arrange
        var lightId = Guid.NewGuid();
        var lights = new Dictionary<Guid, string> { { lightId, "Light1" } };
        var snapshot = new LightSnapshot(lightId, "Light1");
        _discoveryServiceMock.Setup(d => d.GetAllLights()).ReturnsAsync(lights);
        _discoveryServiceMock.Setup(d => d.CaptureLightSnapshotAsync(lightId)).ReturnsAsync(snapshot);
        _discoveryServiceMock.Setup(d => d.SetColorOfLamp(lightId, It.IsAny<HueApi.ColorConverters.RGBColor>())).Returns(Task.CompletedTask);
        _discoveryServiceMock.Setup(d => d.RestoreLightSnapshotAsync(It.IsAny<LightSnapshot>(),0)).Returns(Task.CompletedTask);

        // Act
        var result = await _hueLightService.SetLightColorAsync(lightId, "red");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SetLightColorAsync_HandlesException()
    {
        // Arrange
        var lightId = Guid.NewGuid();
        var lights = new Dictionary<Guid, string> { { lightId, "Light1" } };
        var snapshot = new LightSnapshot(lightId, "Light1");
        _discoveryServiceMock.Setup(d => d.GetAllLights()).ReturnsAsync(lights);
        _discoveryServiceMock.Setup(d => d.CaptureLightSnapshotAsync(lightId)).ReturnsAsync(snapshot);
        _discoveryServiceMock.Setup(d => d.SetColorOfLamp(lightId, It.IsAny<HueApi.ColorConverters.RGBColor>())).ThrowsAsync(new Exception("Bridge error"));
        _discoveryServiceMock.Setup(d => d.RestoreLightSnapshotAsync(It.IsAny<LightSnapshot>(), 0)).Returns(Task.CompletedTask);

        // Act
        var result = await _hueLightService.SetLightColorAsync(lightId, "red");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task FlashLightAsync_ReturnsFalseForEmptyId()
    {
        // Act
        var result = await _hueLightService.FlashLightAsync(Guid.Empty);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task FlashLightAsync_ReturnsFalseWhenNoLights()
    {
        // Arrange
        _discoveryServiceMock.Setup(d => d.GetAllLights()).ReturnsAsync(new Dictionary<Guid, string>());

        // Act
        var result = await _hueLightService.FlashLightAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task FlashLightAsync_ReturnsFalseWhenLightNotFound()
    {
        // Arrange
        var lights = new Dictionary<Guid, string> { { Guid.NewGuid(), "Light1" } };
        _discoveryServiceMock.Setup(d => d.GetAllLights()).ReturnsAsync(lights);

        // Act
        var result = await _hueLightService.FlashLightAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task FlashLightAsync_FlashesSuccessfully()
    {
        // Arrange
        var lightId = Guid.NewGuid();
        var lights = new Dictionary<Guid, string> { { lightId, "Light1" } };
        var snapshot = new LightSnapshot(lightId, "Light1");
        _discoveryServiceMock.Setup(d => d.GetAllLights()).ReturnsAsync(lights);
        _discoveryServiceMock.Setup(d => d.CaptureLightSnapshotAsync(lightId)).ReturnsAsync(snapshot);
        _discoveryServiceMock.Setup(d => d.SetOnState(lightId, true)).Returns(Task.CompletedTask);
        _discoveryServiceMock.Setup(d => d.SetOnState(lightId, false)).Returns(Task.CompletedTask);
        _discoveryServiceMock.Setup(d => d.RestoreLightSnapshotAsync(It.IsAny<LightSnapshot>(),0)).Returns(Task.CompletedTask);

        // Act
        var result = await _hueLightService.FlashLightAsync(lightId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task FlashLightAsync_HandlesException()
    {
        // Arrange
        var lightId = Guid.NewGuid();
        var lights = new Dictionary<Guid, string> { { lightId, "Light1" } };
        var snapshot = new LightSnapshot(lightId, "Light1");
        _discoveryServiceMock.Setup(d => d.GetAllLights()).ReturnsAsync(lights);
        _discoveryServiceMock.Setup(d => d.CaptureLightSnapshotAsync(lightId)).ReturnsAsync(snapshot);
        _discoveryServiceMock.Setup(d => d.SetOnState(lightId, true)).ThrowsAsync(new Exception("Bridge error"));
        _discoveryServiceMock.Setup(d => d.RestoreLightSnapshotAsync(It.IsAny<LightSnapshot>(),0)).Returns(Task.CompletedTask);

        // Act
        var result = await _hueLightService.FlashLightAsync(lightId);

        // Assert
        Assert.False(result);
    }
}