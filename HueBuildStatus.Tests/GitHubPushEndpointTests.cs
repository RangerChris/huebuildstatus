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
        var payloadJson = @"{
  ""ref"": ""refs/heads/main"",
  ""repository"": {
    ""id"": 123456,
    ""name"": ""huebuildstatus""
  },
  ""pusher"": {
    ""name"": ""octocat""
  },
  ""head_commit"": {
    ""id"": ""abc123def456""
  }
}".Replace("\n", "\r\n");
        await using var factory = new ApiWebApplicationFactory();
        var client = factory.CreateClient();
        var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/webhooks/github", content, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}