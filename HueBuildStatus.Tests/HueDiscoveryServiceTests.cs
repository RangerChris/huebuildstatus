using System.Net;
using System.Text;
using FluentAssertions;
using HueBuildStatus.Core;
using Moq;
using Moq.Protected;

namespace HueBuildStatus.Tests;

public class HueDiscoveryServiceTests
{
    [Fact]
    public async Task DiscoverBridgeAsync_ReturnsIp_WhenBridgeFound()
    {
        // Arrange
        var json = "[{\"id\":\"001788fffe09abcd\",\"internalipaddress\":\"192.168.1.2\"}]";
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        var httpClient = new HttpClient(handler.Object);
        var service = new HueDiscoveryService(httpClient);

        // Act
        var ip = await service.DiscoverBridgeAsync();

        // Assert
        ip.Should().Be("192.168.1.2");
    }

    [Fact]
    public async Task DiscoverBridgeAsync_ReturnsNull_WhenNoBridgeFound()
    {
        // Arrange
        var json = "[]";
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        var httpClient = new HttpClient(handler.Object);
        var service = new HueDiscoveryService(httpClient);

        // Act
        var ip = await service.DiscoverBridgeAsync();

        // Assert
        ip.Should().BeNull();
    }

    [Fact]
    public async Task DiscoverBridgeAsync_ReturnsNull_OnException()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new Exception("Network error"));
        var httpClient = new HttpClient(handler.Object);
        var service = new HueDiscoveryService(httpClient);

        // Act
        var ip = await service.DiscoverBridgeAsync();

        // Assert
        ip.Should().BeNull();
    }
}