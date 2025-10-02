using System.Net;
using System.Net.Http.Json;
using HueApi.Models.Clip;
using HueBuildStatus.Core.Features.Hue;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;

namespace HueBuildStatus.Tests;

public class HueEndpointsTests
{
    private readonly ApiWebApplicationFactory _factory = new();

    [Fact]
    public async Task DiscoverBridge_ReturnsOk_WhenIpFound()
    {
        var mockDiscovery = new Mock<IHueDiscoveryService>();
        mockDiscovery.Setup(x => x.DiscoverBridgeAsync()).ReturnsAsync("192.168.1.100");

        using var client = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => { services.AddSingleton<IHueDiscoveryService>(mockDiscovery.Object); })).CreateClient();

        var resp = await client.GetAsync("/hue/discover", TestContext.Current.CancellationToken);

        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<string>(TestContext.Current.CancellationToken);
        body.ShouldBe("192.168.1.100");
    }

    [Fact]
    public async Task DiscoverBridge_ReturnsNotFound_WhenNoIp()
    {
        var mockDiscovery = new Mock<IHueDiscoveryService>();
        mockDiscovery.Setup(x => x.DiscoverBridgeAsync()).ReturnsAsync((string?)null);

        using var client = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => { services.AddSingleton(mockDiscovery.Object); })).CreateClient();

        var resp = await client.GetAsync("/hue/discover", TestContext.Current.CancellationToken);

        resp.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RegisterBridge_ReturnsOk_WhenRegisterReturnsResult()
    {
        var mockDiscovery = new Mock<IHueDiscoveryService>();

        var resultInstance = new RegisterEntertainmentResult();

        mockDiscovery.Setup(x => x.Register(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(resultInstance);

        using var client = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => { services.AddSingleton(mockDiscovery.Object); })).CreateClient();

        var resp = await client.GetAsync("/hue/register?Ip=1.2.3.4&Key=abc", TestContext.Current.CancellationToken);

        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RegisterBridge_ReturnsNotFound_WhenRegisterReturnsNull()
    {
        var mockDiscovery = new Mock<IHueDiscoveryService>();
        mockDiscovery.Setup(x => x.Register(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((RegisterEntertainmentResult?)null);

        using var client = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => { services.AddSingleton(mockDiscovery.Object); })).CreateClient();

        var resp = await client.GetAsync("/hue/register?Ip=1.2.3.4&Key=abc", TestContext.Current.CancellationToken);

        resp.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private class AuthorizationStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.UseAuthorization();
                next(app);
            };
        }
    }
}