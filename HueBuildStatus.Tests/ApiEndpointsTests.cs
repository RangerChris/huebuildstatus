using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace HueBuildStatus.Tests;

public class ApiEndpointsTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public ApiEndpointsTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsOk()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var resp = await client.GetAsync("/health");

        // Assert
        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllLightsEndpoint_ReturnsOkAndJson()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var resp = await client.GetAsync("/hue/GetAllLights");

        // Assert
        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
        resp.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");

        var json = await resp.Content.ReadAsStringAsync();
        json.ShouldNotBeNullOrEmpty();
    }
}