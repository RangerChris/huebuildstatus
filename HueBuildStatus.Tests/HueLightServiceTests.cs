using System;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using Xunit;
using HueBuildStatus.Core.Features.Hue;

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

        var result = await service.GetBridgeIpAsync(null);

        result.ShouldBe("192.168.1.2");
        mockDiscovery.Verify(d => d.DiscoverBridgeAsync(), Times.Once);
    }

    [Fact]
    public async Task GetBridgeIpAsync_ReturnsNull_WhenDiscoveryReturnsNull()
    {
        var mockDiscovery = new Mock<IHueDiscoveryService>();
        mockDiscovery.Setup(d => d.DiscoverBridgeAsync()).ReturnsAsync((string?)null);

        var service = new HueLightService(mockDiscovery.Object);

        var result = await service.GetBridgeIpAsync(null);

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

        var result = await service.RegisterBridgeAsync("1.2.3.4", null);

        result.ShouldBe("new-key");
        mockDiscovery.Verify(d => d.AuthenticateAsync("1.2.3.4", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task RegisterBridgeAsync_ReturnsNull_WhenBridgeIpIsMissing()
    {
        var mockDiscovery = new Mock<IHueDiscoveryService>();
        var service = new HueLightService(mockDiscovery.Object);

        var result = await service.RegisterBridgeAsync("", null);

        result.ShouldBeNull();
        mockDiscovery.Verify(d => d.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}