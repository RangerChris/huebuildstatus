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
        var mockLightSvc = new Mock<IHueLightService>();
        mockLightSvc.Setup(x => x.RegisterBridgeAsync(It.IsAny<string>(), It.IsAny<string?>())).ReturnsAsync("new-key");

        using var client = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => { services.AddSingleton<IHueLightService>(mockLightSvc.Object); })).CreateClient();

        var content = JsonContent.Create(new { Ip = "1.2.3.4", Key = "abc" });
        var resp = await client.PostAsync("/hue/register", content, TestContext.Current.CancellationToken);

        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RegisterBridge_ReturnsNotFound_WhenRegisterReturnsNull()
    {
        var mockLightSvc = new Mock<IHueLightService>();
        mockLightSvc.Setup(x => x.RegisterBridgeAsync(It.IsAny<string>(), It.IsAny<string?>())).ReturnsAsync((string?)null);

        using var client = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => { services.AddSingleton<IHueLightService>(mockLightSvc.Object); })).CreateClient();

        var content = JsonContent.Create(new { Ip = "1.2.3.4", Key = "abc" });
        var resp = await client.PostAsync("/hue/register", content, TestContext.Current.CancellationToken);

        resp.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetLight_ReturnsOk_WhenServiceSetsColor()
    {
        var mockLightSvc = new Mock<IHueLightService>();
        var id = Guid.NewGuid();
        mockLightSvc.Setup(s => s.SetLightColorAsync(id, "green", It.IsAny<int>())).ReturnsAsync(true);

        using var client = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => { services.AddSingleton<IHueLightService>(mockLightSvc.Object); })).CreateClient();

        var content = JsonContent.Create(new { LightId = id, ColorName = "green", DurationMs = 10 });
        var resp = await client.PostAsync("/hue/SetLight", content, TestContext.Current.CancellationToken);

        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SetLight_ReturnsNotFound_WhenServiceFails()
    {
        var mockLightSvc = new Mock<IHueLightService>();
        var id = Guid.NewGuid();
        mockLightSvc.Setup(s => s.SetLightColorAsync(id, "red", It.IsAny<int>())).ReturnsAsync(false);

        using var client = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => { services.AddSingleton<IHueLightService>(mockLightSvc.Object); })).CreateClient();

        var content = JsonContent.Create(new { LightId = id, ColorName = "red", DurationMs = 10 });
        var resp = await client.PostAsync("/hue/SetLight", content, TestContext.Current.CancellationToken);

        resp.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetLight_ReturnsBadRequest_WhenLightIdEmpty()
    {
        using var client = _factory.CreateClient();

        var content = JsonContent.Create(new { LightId = Guid.Empty, ColorName = "green", DurationMs = 10 });
        var resp = await client.PostAsync("/hue/SetLight", content, TestContext.Current.CancellationToken);

        resp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SetPulsatingLight_ReturnsOk_WhenServicePulsates()
    {
        var mockLightSvc = new Mock<IHueLightService>();
        var id = Guid.NewGuid();
        mockLightSvc.Setup(s => s.FlashLightAsync(id, It.IsAny<int>())).ReturnsAsync(true);

        using var client = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => { services.AddSingleton<IHueLightService>(mockLightSvc.Object); })).CreateClient();

        var content = JsonContent.Create(new { LightId = id, DurationMs = 100 });
        var resp = await client.PostAsync("/hue/SetPulsatingLight", content, TestContext.Current.CancellationToken);

        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SetPulsatingLight_ReturnsNotFound_WhenServiceFails()
    {
        var mockLightSvc = new Mock<IHueLightService>();
        var id = Guid.NewGuid();
        mockLightSvc.Setup(s => s.FlashLightAsync(id, It.IsAny<int>())).ReturnsAsync(false);

        using var client = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => { services.AddSingleton<IHueLightService>(mockLightSvc.Object); })).CreateClient();

        var content = JsonContent.Create(new { LightId = id, DurationMs = 100 });
        var resp = await client.PostAsync("/hue/SetPulsatingLight", content, TestContext.Current.CancellationToken);

        resp.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetPulsatingLight_ReturnsBadRequest_WhenLightIdEmpty()
    {
        using var client = _factory.CreateClient();

        var content = JsonContent.Create(new { LightId = Guid.Empty, DurationMs = 100 });
        var resp = await client.PostAsync("/hue/SetPulsatingLight", content, TestContext.Current.CancellationToken);

        resp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
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