using System.Net;
using System.Text;
using Shouldly;

namespace HueBuildStatus.Tests;

public class GitHubPushEndpointTests
{
    [Fact]
    public async Task HandleAsync_Integration_LogsPayloadAndReturnsOk()
    {
        // Arrange
        const string payloadJson = """{  "ref": "refs/heads/main",  "repository": {    "id": 123456,    "name": "huebuildstatus"  },  "pusher": {    "name": "octocat"  },  "head_commit": {    "id": "abc123def456"  }}""";
        await using var factory = new ApiWebApplicationFactory();
        var client = factory.CreateClient();
        var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/webhooks/github", content, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HandleAsync_HandlesLargePayloads()
    {
        // Arrange
        var largePayload = "{ \"data\": \"" + new string('a', 1_000) + "\" }"; // JSON payload
        await using var factory = new ApiWebApplicationFactory();
        var client = factory.CreateClient();
        var content = new StringContent(largePayload, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/webhooks/github", content, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HandleAsync_HandlesMalformedJson()
    {
        // Arrange
        var malformedJson = "{ invalid json }";
        await using var factory = new ApiWebApplicationFactory();
        var client = factory.CreateClient();
        var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");


        // Act
        var response = await client.PostAsync("/webhooks/github", content, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task HandleAsync_HandlesEmptyPayload()
    {
        // Arrange
        var emptyPayload = "{}"; // Empty JSON object
        await using var factory = new ApiWebApplicationFactory();
        var client = factory.CreateClient();
        var content = new StringContent(emptyPayload, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/webhooks/github", content, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}