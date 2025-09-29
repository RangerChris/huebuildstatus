using System.Net;
using System.Text.Json;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using HueBuildStatus.Core.Features.Hue;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Xunit;
using HueApi.Models.Clip;

namespace HueBuildStatus.Tests;

public class HueEndpointsTests
{
    private readonly ApiWebApplicationFactory _factory = new();

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

    [Fact]
    public async Task DiscoverBridge_ReturnsOk_WhenIpFound()
    {
        var mockDiscovery = new Mock<IHueDiscoveryService>();
        mockDiscovery.Setup(x => x.DiscoverBridgeAsync()).ReturnsAsync("192.168.1.100");

        using var client = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IHueDiscoveryService>(mockDiscovery.Object);
            })).CreateClient();

        var resp = await client.GetAsync("/hue/discover");

        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<string>();
        body.ShouldBe("192.168.1.100");
    }

    [Fact]
    public async Task DiscoverBridge_ReturnsNotFound_WhenNoIp()
    {
        var mockDiscovery = new Mock<IHueDiscoveryService>();
        mockDiscovery.Setup(x => x.DiscoverBridgeAsync()).ReturnsAsync((string?)null);

        using var client = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IHueDiscoveryService>(mockDiscovery.Object);
            })).CreateClient();

        var resp = await client.GetAsync("/hue/discover");

        resp.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetLight_ReturnsOk()
    {
        var mockLightService = new Mock<IHueLightService>();

        using var client = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IHueLightService>(mockLightService.Object);
                services.AddAuthorization();
                services.AddSingleton<IStartupFilter>(new AuthorizationStartupFilter());
            })).CreateClient();

        var resp = await client.GetAsync("/hue/getLight?lightName=my-light");

        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var json = await resp.Content.ReadAsStringAsync();
        json.ShouldNotBeNullOrEmpty();
        json.ShouldContain("light", Case.Insensitive);
    }

    [Fact]
    public async Task RegisterBridge_ReturnsOk_WhenRegisterReturnsResult()
    {
        var mockDiscovery = new Mock<IHueDiscoveryService>();

        var resultInstance = new RegisterEntertainmentResult();

        mockDiscovery.Setup(x => x.Register(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(resultInstance);

        using var client = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IHueDiscoveryService>(mockDiscovery.Object);
            })).CreateClient();

        var resp = await client.GetAsync("/hue/register?Ip=1.2.3.4&Key=abc");

        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RegisterBridge_ReturnsNotFound_WhenRegisterReturnsNull()
    {
        var mockDiscovery = new Mock<IHueDiscoveryService>();
        mockDiscovery.Setup(x => x.Register(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((RegisterEntertainmentResult?)null);

        using var client = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IHueDiscoveryService>(mockDiscovery.Object);
            })).CreateClient();

        var resp = await client.GetAsync("/hue/register?Ip=1.2.3.4&Key=abc");

        resp.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetLight_ReturnsOk()
    {
        var mockLightService = new Mock<IHueLightService>();

        using var client = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IHueLightService>(mockLightService.Object);
                services.AddAuthorization();
                services.AddSingleton<IStartupFilter>(new AuthorizationStartupFilter());
            })).CreateClient();

        var payload = JsonSerializer.Serialize(new { Color = (object?)null, Light = (object?)null });
        var resp = await client.PostAsync("/hue/SetLight", new StringContent(payload, System.Text.Encoding.UTF8, "application/json"));

        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SetPulsatingLight_ReturnsOk()
    {
        var mockLightService = new Mock<IHueLightService>();

        using var client = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IHueLightService>(mockLightService.Object);
                services.AddAuthorization();
                services.AddSingleton<IStartupFilter>(new AuthorizationStartupFilter());
            })).CreateClient();

        var payload = JsonSerializer.Serialize(new { Color = (object?)null, Light = (object?)null });
        var resp = await client.PostAsync("/hue/SetPulsatingLight", new StringContent(payload, System.Text.Encoding.UTF8, "application/json"));

        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}