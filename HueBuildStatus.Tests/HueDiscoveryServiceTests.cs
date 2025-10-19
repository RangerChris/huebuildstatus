using System.Net;
using System.Text;
using HueApi.ColorConverters;
using HueBuildStatus.Core.Features.Config;
using HueBuildStatus.Core.Features.Hue;
using Moq;
using Moq.Protected;
using Shouldly;

namespace HueBuildStatus.Tests;

public class HueDiscoveryServiceTests
{
    [Fact]
    public async Task DiscoverBridgeAsync_ReturnsIp_WhenDiscoveryReturnsValidJson()
    {
        // Arrange
        var json = "[{\"id\":\"bridge-1\",\"internalipaddress\":\"192.168.1.2\"}]";
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handler.Object);
        var service = new HueDiscoveryService(httpClient);

        // Act
        var result = await service.DiscoverBridgeAsync();

        // Assert
        result.ShouldBe("192.168.1.2");
    }

    [Fact]
    public async Task AuthenticateAsync_ReturnsUsername_WhenSuccessResponse()
    {
        // Arrange
        var json = "[{\"success\":{\"username\":\"test-appkey\",\"clientkey\":\"abc\"}}]";
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handler.Object);
        var service = new HueDiscoveryService(httpClient);

        // Act
        var result = await service.AuthenticateAsync("192.168.1.2", "my-device");

        // Assert
        result.ShouldBe("test-appkey");
    }

    [Fact]
    public async Task AuthenticateAsync_ReturnsNull_WhenErrorResponse()
    {
        // Arrange
        var json = "[{\"error\":{\"type\":101,\"description\":\"link button not pressed\"}}]";
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handler.Object);
        var service = new HueDiscoveryService(httpClient);

        // Act
        var result = await service.AuthenticateAsync("192.168.1.2", "my-device");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task DiscoverBridgeAsync_ReturnsNull_OnException()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new Exception("Network failure"));

        var httpClient = new HttpClient(handler.Object);
        var service = new HueDiscoveryService(httpClient);

        // Act
        var result = await service.DiscoverBridgeAsync();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task DiscoverBridgeAsync_ReturnsConfiguredIp_WithoutCallingDiscovery()
    {
        // Arrange
        var configMock = new Mock<IAppConfiguration>();
        configMock.Setup(c => c.BridgeIp).Returns("192.168.1.100");

        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new Exception("Should not be called"));

        var httpClient = new HttpClient(handler.Object);
        var service = new HueDiscoveryService(httpClient, config: configMock.Object);

        // Act
        var result = await service.DiscoverBridgeAsync();

        // Assert
        result.ShouldBe("192.168.1.100");
    }

    [Fact]
    public async Task Register_ThrowsArgumentNullException_WhenBridgeIpIsNull()
    {
        // Arrange
        var service = new HueDiscoveryService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.Register(null!, "user"));
    }

    [Fact]
    public async Task Register_ThrowsArgumentNullException_WhenBridgeIpIsEmpty()
    {
        // Arrange
        var service = new HueDiscoveryService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.Register("", "user"));
    }

    [Fact]
    public async Task Register_ThrowsArgumentNullException_WhenHueUserIsNull()
    {
        // Arrange
        var service = new HueDiscoveryService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.Register("192.168.1.1", null!));
    }

    [Fact]
    public async Task Register_ThrowsArgumentNullException_WhenHueUserIsEmpty()
    {
        // Arrange
        var service = new HueDiscoveryService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.Register("192.168.1.1", ""));
    }

    [Fact]
    public async Task PulsateAsync_ThrowsArgumentNullException_WhenLightIsNull()
    {
        // Arrange
        var service = new HueDiscoveryService();
        RGBColor color = new RGBColor(255, 0, 0);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.PulsateAsync(null!, color, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RestoreLightSnapshotAsync_ThrowsArgumentNullException_WhenSnapshotIsNull()
    {
        // Arrange
        var service = new HueDiscoveryService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.RestoreLightSnapshotAsync(null!));
    }

    [Fact]
    public async Task DiscoverBridgeAsync_ReturnsNull_WhenResponseIsNotArray()
    {
        // Arrange
        var json = "{}";
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handler.Object);
        var service = new HueDiscoveryService(httpClient);

        // Act
        var result = await service.DiscoverBridgeAsync();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task DiscoverBridgeAsync_ReturnsNull_WhenArrayIsEmpty()
    {
        // Arrange
        var json = "[]";
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handler.Object);
        var service = new HueDiscoveryService(httpClient);

        // Act
        var result = await service.DiscoverBridgeAsync();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task DiscoverBridgeAsync_ReturnsNull_WhenNoInternalIpAddress()
    {
        // Arrange
        var json = "[{\"id\":\"bridge-1\"}]";
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handler.Object);
        var service = new HueDiscoveryService(httpClient);

        // Act
        var result = await service.DiscoverBridgeAsync();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_ReturnsNull_WhenResponseIsNotArray()
    {
        // Arrange
        var json = "{}";
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handler.Object);
        var service = new HueDiscoveryService(httpClient);

        // Act
        var result = await service.AuthenticateAsync("192.168.1.2", "my-device");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_ReturnsNull_WhenNoSuccessOrError()
    {
        // Arrange
        var json = "[{}]";
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handler.Object);
        var service = new HueDiscoveryService(httpClient);

        // Act
        var result = await service.AuthenticateAsync("192.168.1.2", "my-device");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAllLights_ReturnsNull_WhenClientCannotBeCreated()
    {
        // Arrange
        var configMock = new Mock<IAppConfiguration>();
        // No BridgeIp or BridgeKey set

        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new Exception("Should not be called"));

        var httpClient = new HttpClient(handler.Object);
        var service = new HueDiscoveryService(httpClient, config: configMock.Object);

        // Act
        var result = await service.GetAllLights();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task CaptureLightSnapshotAsync_ThrowsInvalidOperationException_WhenClientCannotBeCreated()
    {
        // Arrange
        var configMock = new Mock<IAppConfiguration>();
        // No BridgeIp or BridgeKey set

        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new Exception("Should not be called"));

        var httpClient = new HttpClient(handler.Object);
        var service = new HueDiscoveryService(httpClient, config: configMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CaptureLightSnapshotAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task RestoreLightSnapshotAsync_ThrowsInvalidOperationException_WhenClientCannotBeCreated()
    {
        // Arrange
        var configMock = new Mock<IAppConfiguration>();
        // No BridgeIp or BridgeKey set

        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new Exception("Should not be called"));

        var httpClient = new HttpClient(handler.Object);
        var service = new HueDiscoveryService(httpClient, config: configMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RestoreLightSnapshotAsync(new LightSnapshot(Guid.NewGuid(), "{}")));
    }
}