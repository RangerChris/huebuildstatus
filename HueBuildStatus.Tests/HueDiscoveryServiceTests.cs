using System.Net;
using System.Text;
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
}