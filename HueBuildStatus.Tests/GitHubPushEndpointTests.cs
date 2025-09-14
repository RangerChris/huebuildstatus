// New tests for the GitHub push webhook endpoint

using System.Net.Http.Json;
using FluentAssertions;
using HueBuildStatus.Core.Features.Webhooks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace HueBuildStatus.Tests;

public class GitHubPushEndpointTests
{
    [Fact]
    public async Task Post_To_GitHubPushEndpoint_InvokesHandler()
    {
        // Arrange
        var handlerMock = new Mock<IGitHubWebhookHandler>();
        handlerMock
            .Setup(h => h.HandlePushAsync(It.IsAny<GitHubPushPayload>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace or add the IGitHubWebhookHandler with our mock
                services.RemoveAll<IGitHubWebhookHandler>();
                services.AddSingleton<IGitHubWebhookHandler>(handlerMock.Object);
            });
        });

        var client = factory.CreateClient();

        var payload = new GitHubPushPayload
        {
            Ref = "refs/heads/main",
            RepositoryName = "example-repo",
            PusherName = "alice",
            HeadCommitId = "abc123"
        };

        // Act
        var resp = await client.PostAsJsonAsync("/webhooks/github/push", payload, TestContext.Current.CancellationToken);

        // Assert
        resp.IsSuccessStatusCode.Should().BeTrue();
        handlerMock.Verify(h => h.HandlePushAsync(It.Is<GitHubPushPayload>(p => p.HeadCommitId == "abc123"), It.IsAny<CancellationToken>()), Times.Once);
    }
}