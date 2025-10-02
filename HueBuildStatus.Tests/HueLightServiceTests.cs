using HueBuildStatus.Core.Features.Hue;
using Moq;
using Shouldly;

namespace HueBuildStatus.Tests;

public class HueLightServiceTests
{
    [Fact]
    public async Task GetBridgeIpAsync_ReturnsConfiguredIp_WhenProvided()
    {
        var mockDiscovery = new Mock<IHueDiscoveryService>();
        var service = new HueLightService(mockDiscovery.Object);

        var result = await service.GetBridgeIpAsync("10.0.0.5");

        result.ShouldBe("10.0.0.5");
        mockDiscovery.Verify(d => d.DiscoverBridgeAsync(), Times.Never);
    }

    [Fact]
    public async Task GetBridgeIpAsync_UsesDiscovery_WhenConfiguredIsNull()
    {
        var mockDiscovery = new Mock<IHueDiscoveryService>();
        mockDiscovery.Setup(d => d.DiscoverBridgeAsync()).ReturnsAsync("192.168.1.2");

        var service = new HueLightService(mockDiscovery.Object);

        var result = await service.GetBridgeIpAsync();

        result.ShouldBe("192.168.1.2");
        mockDiscovery.Verify(d => d.DiscoverBridgeAsync(), Times.Once);
    }

    [Fact]
    public async Task GetBridgeIpAsync_ReturnsNull_WhenDiscoveryReturnsNull()
    {
        var mockDiscovery = new Mock<IHueDiscoveryService>();
        mockDiscovery.Setup(d => d.DiscoverBridgeAsync()).ReturnsAsync((string?)null);

        var service = new HueLightService(mockDiscovery.Object);

        var result = await service.GetBridgeIpAsync();

        result.ShouldBeNull();
        mockDiscovery.Verify(d => d.DiscoverBridgeAsync(), Times.Once);
    }

    [Fact]
    public async Task RegisterBridgeAsync_ReturnsConfiguredKey_WhenProvided()
    {
        var mockDiscovery = new Mock<IHueDiscoveryService>();
        var service = new HueLightService(mockDiscovery.Object);

        var result = await service.RegisterBridgeAsync("1.2.3.4", "existing-key");

        result.ShouldBe("existing-key");
        mockDiscovery.Verify(d => d.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterBridgeAsync_UsesAuthenticate_WhenNoConfiguredKey()
    {
        var mockDiscovery = new Mock<IHueDiscoveryService>();
        mockDiscovery.Setup(d => d.AuthenticateAsync("1.2.3.4", It.IsAny<string>())).ReturnsAsync("new-key");

        var service = new HueLightService(mockDiscovery.Object);

        var result = await service.RegisterBridgeAsync("1.2.3.4");

        result.ShouldBe("new-key");
        mockDiscovery.Verify(d => d.AuthenticateAsync("1.2.3.4", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task RegisterBridgeAsync_ReturnsNull_WhenBridgeIpIsMissing()
    {
        var mockDiscovery = new Mock<IHueDiscoveryService>();
        var service = new HueLightService(mockDiscovery.Object);

        var result = await service.RegisterBridgeAsync("");

        result.ShouldBeNull();
        mockDiscovery.Verify(d => d.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetAllLightsAsync_ReturnsLights_WhenDiscoveryReturns()
    {
        var mockDiscovery = new Mock<IHueDiscoveryService>();
        var id = Guid.NewGuid();
        var lights = new Dictionary<Guid, string> { { id, "Desk" } };
        mockDiscovery.Setup(d => d.GetAllLights()).ReturnsAsync(lights);

        var service = new HueLightService(mockDiscovery.Object);

        var result = await service.GetAllLightsAsync();

        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[id].ShouldBe("Desk");
        mockDiscovery.Verify(d => d.GetAllLights(), Times.Once);
    }

    [Fact]
    public async Task GetAllLightsAsync_ReturnsEmpty_WhenDiscoveryReturnsNull()
    {
        var mockDiscovery = new Mock<IHueDiscoveryService>();
        // Return a Task with a null Dictionary to match the nullable return type
        mockDiscovery.Setup(d => d.GetAllLights()).Returns(Task.FromResult<Dictionary<Guid, string>?>(null));

        var service = new HueLightService(mockDiscovery.Object);

        var result = await service.GetAllLightsAsync();

        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
        mockDiscovery.Verify(d => d.GetAllLights(), Times.Once);
    }

    [Fact]
    public async Task GetLightByNameAsync_ReturnsLight_WhenFound_CaseInsensitive()
    {
        var mockDiscovery = new Mock<IHueDiscoveryService>();
        var id = Guid.NewGuid();
        var lights = new Dictionary<Guid, string> { { id, "Desk Lamp" } };
        mockDiscovery.Setup(d => d.GetAllLights()).ReturnsAsync(lights);

        var service = new HueLightService(mockDiscovery.Object);

        var result = await service.GetLightByNameAsync("desk lamp");

        result.ShouldNotBeNull();
        result.Id.ShouldBe(id);
        result.Name.ShouldBe("Desk Lamp");
        mockDiscovery.Verify(d => d.GetAllLights(), Times.Once);
    }

    [Fact]
    public async Task GetLightByNameAsync_ReturnsNull_WhenNotFound()
    {
        var mockDiscovery = new Mock<IHueDiscoveryService>();
        var lights = new Dictionary<Guid, string> { { Guid.NewGuid(), "Other" } };
        mockDiscovery.Setup(d => d.GetAllLights()).ReturnsAsync(lights);

        var service = new HueLightService(mockDiscovery.Object);

        var result = await service.GetLightByNameAsync("Nonexistent");

        result.ShouldBeNull();
        mockDiscovery.Verify(d => d.GetAllLights(), Times.Once);
    }

    [Fact]
    public async Task GetLightByNameAsync_ReturnsNull_WhenNameIsNullOrWhitespace()
    {
        var mockDiscovery = new Mock<IHueDiscoveryService>();
        var service = new HueLightService(mockDiscovery.Object);

        var result = await service.GetLightByNameAsync("   ");

        result.ShouldBeNull();
        mockDiscovery.Verify(d => d.GetAllLights(), Times.Never);
    }

    [Fact]
    public async Task GetLightByNameAsync_ReturnsNull_WhenDiscoveryReturnsNull()
    {
        var mockDiscovery = new Mock<IHueDiscoveryService>();
        mockDiscovery.Setup(d => d.GetAllLights()).Returns(Task.FromResult<Dictionary<Guid, string>?>(null));

        var service = new HueLightService(mockDiscovery.Object);

        var result = await service.GetLightByNameAsync("Desk");

        result.ShouldBeNull();
        mockDiscovery.Verify(d => d.GetAllLights(), Times.Once);
    }

    [Fact]
    public async Task CaptureLightSnapshotAsync_ReturnsSnapshot_WhenDiscoveryReturns()
    {
        var mockDiscovery = new Mock<IHueDiscoveryService>();
        var id = Guid.NewGuid();
        var lights = new Dictionary<Guid, string> { { id, "Desk" } };
        mockDiscovery.Setup(d => d.GetAllLights()).ReturnsAsync(lights);
        var snapshot = new LightSnapshot(id, "{\"on\":true}", DateTime.UtcNow);
        mockDiscovery.Setup(d => d.CaptureLightSnapshotAsync(id)).ReturnsAsync(snapshot);

        var service = new HueLightService(mockDiscovery.Object);

        var result = await service.CaptureLightSnapshotAsync(id);

        result.ShouldNotBeNull();
        result!.LightId.ShouldBe(id);
        result.JsonSnapshot.ShouldBe("{\"on\":true}");
        mockDiscovery.Verify(d => d.CaptureLightSnapshotAsync(id), Times.Once);
    }

    [Fact]
    public async Task CaptureLightSnapshotAsync_ReturnsNull_WhenLightNotFound()
    {
        var mockDiscovery = new Mock<IHueDiscoveryService>();
        var existingId = Guid.NewGuid();
        var lights = new Dictionary<Guid, string> { { existingId, "Other" } };
        mockDiscovery.Setup(d => d.GetAllLights()).ReturnsAsync(lights);

        var service = new HueLightService(mockDiscovery.Object);

        var result = await service.CaptureLightSnapshotAsync(Guid.NewGuid());

        result.ShouldBeNull();
        mockDiscovery.Verify(d => d.CaptureLightSnapshotAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task CaptureLightSnapshotAsync_ReturnsNull_WhenLightIdIsEmpty()
    {
        var mockDiscovery = new Mock<IHueDiscoveryService>();
        var service = new HueLightService(mockDiscovery.Object);

        var result = await service.CaptureLightSnapshotAsync(Guid.Empty);

        result.ShouldBeNull();
        mockDiscovery.Verify(d => d.GetAllLights(), Times.Never);
        mockDiscovery.Verify(d => d.CaptureLightSnapshotAsync(It.IsAny<Guid>()), Times.Never);
    }
}