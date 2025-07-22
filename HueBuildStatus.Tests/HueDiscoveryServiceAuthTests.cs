using System.Net;
using System.Text;
using FluentAssertions;
using HueBuildStatus.Core;
using Moq;
using Moq.Protected;

namespace HueBuildStatus.Tests;

public class HueDiscoveryServiceAuthTests
{
    [Fact]
    public async Task AuthenticateAsync_ReturnsAppKey_WhenSuccess()
    {
        // Arrange
        var json = "[{\"success\":{\"username\":\"test-appkey\"}}]";
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
        var appkey = await service.AuthenticateAsync("192.168.1.2", "myapp#dev1");

        // Assert
        appkey.Should().Be("test-appkey");
    }

    [Fact]
    public async Task AuthenticateAsync_ReturnsNull_WhenError()
    {
        // Arrange
        var json = "[{\"error\":{\"type\":101,\"description\":\"link button not pressed\"}}]";
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
        var appkey = await service.AuthenticateAsync("192.168.1.2", "myapp#dev1");

        // Assert
        appkey.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_ReturnsNull_OnException()
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
        var appkey = await service.AuthenticateAsync("192.168.1.2", "myapp#dev1");

        // Assert
        appkey.Should().BeNull();
    }
}
