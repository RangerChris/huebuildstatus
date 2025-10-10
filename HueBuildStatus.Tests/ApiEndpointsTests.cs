using System.Net;
using System.Text;
using HueBuildStatus.Core.Features.Hue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;

namespace HueBuildStatus.Tests;

public class ApiEndpointsTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public ApiEndpointsTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsOk_WhenConfigured()
    {
        // Arrange: provide bridgeIp and bridgeKey in configuration
        var configuredFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, conf) =>
            {
                conf.AddInMemoryCollection([
                    new KeyValuePair<string, string?>("bridgeIp", "192.168.1.100"),
                    new KeyValuePair<string, string?>("bridgeKey", "test-key")
                ]);
            });
        });

        using var client = configuredFactory.CreateClient();

        // Act
        var resp = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        // Assert
        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var json = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        json.ShouldContain("Healthy");
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsServiceUnavailable_WhenNotConfigured()
    {
        // Arrange: use default factory with no configuration overrides
        using var client = _factory.CreateClient();

        // Act
        var resp = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        // Assert
        resp.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        var json = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        json.ShouldContain("Unhealthy");
        json.ShouldContain("bridgeIp");
        json.ShouldContain("bridgeKey");
    }

    [Fact]
    public async Task GetAllLightsEndpoint_ReturnsOkAndJson()
    {
        var mockLightSvc = new Mock<IHueLightService>();
        var id = Guid.NewGuid();
        var lights = new Dictionary<Guid, string> { { id, "Desk" } };
        mockLightSvc.Setup(s => s.GetAllLightsAsync()).ReturnsAsync(lights);

        var configuredFactory = _factory.WithWebHostBuilder(builder => { builder.ConfigureServices(services => { services.AddSingleton(mockLightSvc.Object); }); });

        using var client = configuredFactory.CreateClient();

        var resp = await client.GetAsync("/hue/GetAllLights", TestContext.Current.CancellationToken);

        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
        resp.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");

        var json = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        json.ShouldContain("Desk");
    }

    [Fact]
    public async Task DiscoverBridge_ReturnsOkWithIp_WhenFound()
    {
        var mockLightSvc = new Mock<IHueLightService>();
        mockLightSvc.Setup(s => s.GetBridgeIpAsync(It.IsAny<string?>())).ReturnsAsync("192.168.1.2");

        var configuredFactory = _factory.WithWebHostBuilder(builder => { builder.ConfigureServices(services => { services.AddSingleton(mockLightSvc.Object); }); });

        using var client = configuredFactory.CreateClient();

        var resp = await client.GetAsync("/hue/discover", TestContext.Current.CancellationToken);

        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.ShouldBe("\"192.168.1.2\"");
    }

    [Fact]
    public async Task DiscoverBridge_ReturnsNotFound_WhenNotFound()
    {
        var mockLightSvc = new Mock<IHueLightService>();
        mockLightSvc.Setup(s => s.GetBridgeIpAsync(It.IsAny<string?>())).ReturnsAsync((string?)null);

        var configuredFactory = _factory.WithWebHostBuilder(builder => { builder.ConfigureServices(services => { services.AddSingleton(mockLightSvc.Object); }); });

        using var client = configuredFactory.CreateClient();

        var resp = await client.GetAsync("/hue/discover", TestContext.Current.CancellationToken);

        resp.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RegisterBridge_ReturnsBadRequest_WhenIpMissing()
    {
        var configuredFactory = _factory.WithWebHostBuilder(_ => { });
        using var client = configuredFactory.CreateClient();

        var content = new StringContent("{ \"Ip\": \"\", \"Key\": \"k\" }", Encoding.UTF8, "application/json");
        var resp = await client.PostAsync("/hue/register", content, TestContext.Current.CancellationToken);

        resp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterBridge_ReturnsOkWithKey_WhenRegistered()
    {
        var mockLightSvc = new Mock<IHueLightService>();
        mockLightSvc.Setup(s => s.RegisterBridgeAsync(It.IsAny<string>(), It.IsAny<string?>())).ReturnsAsync("new-key");

        var configuredFactory = _factory.WithWebHostBuilder(builder => { builder.ConfigureServices(services => { services.AddSingleton(mockLightSvc.Object); }); });

        using var client = configuredFactory.CreateClient();

        var content = new StringContent("{ \"Ip\": \"1.2.3.4\", \"Key\": null }", Encoding.UTF8, "application/json");
        var resp = await client.PostAsync("/hue/register", content, TestContext.Current.CancellationToken);

        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.ShouldBe("\"new-key\"");
    }

    [Fact]
    public async Task RegisterBridge_ReturnsNotFound_WhenRegistrationFails()
    {
        var mockLightSvc = new Mock<IHueLightService>();
        mockLightSvc.Setup(s => s.RegisterBridgeAsync(It.IsAny<string>(), It.IsAny<string?>())).ReturnsAsync((string?)null);

        var configuredFactory = _factory.WithWebHostBuilder(builder => { builder.ConfigureServices(services => { services.AddSingleton(mockLightSvc.Object); }); });

        using var client = configuredFactory.CreateClient();

        var content = new StringContent("{ \"Ip\": \"1.2.3.4\", \"Key\": null }", Encoding.UTF8, "application/json");
        var resp = await client.PostAsync("/hue/register", content, TestContext.Current.CancellationToken);

        resp.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetLight_ReturnsOk_WhenFound()
    {
        var mockLightSvc = new Mock<IHueLightService>();
        var id = Guid.NewGuid();
        var info = new LightInfo { Id = id, Name = "Desk Lamp" };
        mockLightSvc.Setup(s => s.GetLightByNameAsync(It.IsAny<string>())).ReturnsAsync(info);

        var configuredFactory = _factory.WithWebHostBuilder(builder => { builder.ConfigureServices(services => { services.AddSingleton(mockLightSvc.Object); }); });

        using var client = configuredFactory.CreateClient();

        var resp = await client.GetAsync("/hue/getlight?lightName=Desk Lamp", TestContext.Current.CancellationToken);

        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var json = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        json.ShouldContain("Desk Lamp");
    }

    [Fact]
    public async Task GetLight_ReturnsNotFound_WhenNotFound()
    {
        var mockLightSvc = new Mock<IHueLightService>();
        mockLightSvc.Setup(s => s.GetLightByNameAsync(It.IsAny<string>())).ReturnsAsync((LightInfo?)null);

        var configuredFactory = _factory.WithWebHostBuilder(builder => { builder.ConfigureServices(services => { services.AddSingleton(mockLightSvc.Object); }); });

        using var client = configuredFactory.CreateClient();

        var resp = await client.GetAsync("/hue/getlight?lightName=Missing", TestContext.Current.CancellationToken);

        resp.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetLight_ReturnsBadRequest_WhenNameMissing()
    {
        using var client = _factory.CreateClient();

        var resp = await client.GetAsync("/hue/getlight?lightName=   ", TestContext.Current.CancellationToken);

        resp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}