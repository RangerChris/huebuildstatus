using System.Net.Http.Json;
using FluentAssertions;
using HueBuildStatus.Api;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HueBuildStatus.Tests;

public class GitHubPushEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public GitHubPushEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_Return_200_And_Message_On_Valid_Push_Event()
    {
        var payload = new GitHubPushEvent
        {
            Ref = "refs/heads/main",
            Before = "abc",
            After = "def",
            Repository = new GitHubRepository { Name = "huebuildstatus" },
            Pusher = new GitHubPusher { Name = "RangerChris" }
        };
        var response = await _factory.CreateDefaultClient()
            .PostAsJsonAsync("/webhook/github/push", payload, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        json.Should().Contain("Received push for repo: huebuildstatus");
    }
}
