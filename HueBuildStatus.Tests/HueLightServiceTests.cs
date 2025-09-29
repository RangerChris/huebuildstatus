using System.Net;
using System.Text;
using Shouldly;
using HueBuildStatus.Core.Features.Hue;
using Moq;
using Moq.Protected;

namespace HueBuildStatus.Tests;

public class HueLightServiceTests
{
    [Fact]
    public async Task SetLightOnOffAsync_ReturnsTrue_WhenSuccess()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });
        var httpClient = new HttpClient(handler.Object);
        var service = new HueLightService(httpClient);

        // Act
        var result = await service.SetLightOnOffAsync("192.168.1.2", "test-appkey", "1", true);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task SetLightOnOffAsync_ReturnsFalse_WhenFailure()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });
        var httpClient = new HttpClient(handler.Object);
        var service = new HueLightService(httpClient);

        // Act
        var result = await service.SetLightOnOffAsync("192.168.1.2", "test-appkey", "1", false);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task SetLightColorAsync_ReturnsTrue_WhenSuccess()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });
        var httpClient = new HttpClient(handler.Object);
        var service = new HueLightService(httpClient);

        // Act
        var result = await service.SetLightColorAsync("192.168.1.2", "test-appkey", "1", "#FF0000");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task SetLightColorAsync_ReturnsFalse_WhenFailure()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });
        var httpClient = new HttpClient(handler.Object);
        var service = new HueLightService(httpClient);

        // Act
        var result = await service.SetLightColorAsync("192.168.1.2", "test-appkey", "1", "#FF0000");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task SetLightBrightnessAsync_ReturnsTrue_WhenSuccess()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });
        var httpClient = new HttpClient(handler.Object);
        var service = new HueLightService(httpClient);

        // Act
        var result = await service.SetLightBrightnessAsync("192.168.1.2", "test-appkey", "1", 50);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task SetLightBrightnessAsync_ReturnsFalse_WhenFailure()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });
        var httpClient = new HttpClient(handler.Object);
        var service = new HueLightService(httpClient);

        // Act
        var result = await service.SetLightBrightnessAsync("192.168.1.2", "test-appkey", "1", 50);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task SetLightOnOffAsync_ReturnsFalse_OnException()
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
        var service = new HueLightService(httpClient);

        // Act
        var result = await service.SetLightOnOffAsync("192.168.1.2", "test-appkey", "1", true);

        // Assert
        result.ShouldBeFalse();
    }
}