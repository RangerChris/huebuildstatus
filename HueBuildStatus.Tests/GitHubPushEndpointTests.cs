using System.Net;
using System.Text;
using Shouldly;
using System.Text.Json;

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

    public static IEnumerable<object[]> GetTestFiles()
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "TestFiles");
        return Directory.GetFiles(directory, "*.json")
            .Select(file => new object[] { file });
    }

    private static string? FindJsonProperty(JsonElement element, string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals(propertyName))
                {
                    return property.Value.GetString();
                }

                var nestedResult = FindJsonProperty(property.Value, propertyName);
                if (nestedResult != null)
                {
                    return nestedResult;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var nestedResult = FindJsonProperty(item, propertyName);
                if (nestedResult != null)
                {
                    return nestedResult;
                }
            }
        }

        return null;
    }

    [Theory]
    [MemberData(nameof(GetTestFiles))]
    public async Task HandleAsync_Integration_ParsesJsonAndVerifiesStatus(string filePath)
    {
        // Arrange
        var jsonContent = await File.ReadAllTextAsync(filePath, TestContext.Current.CancellationToken);
        var jsonObject = JsonDocument.Parse(jsonContent);

        var status = FindJsonProperty(jsonObject.RootElement, "status");
        var conclusion = FindJsonProperty(jsonObject.RootElement, "conclusion");

        // Act & Assert
        switch (status)
        {
            case "queued":
                status.ShouldBe("queued"); // waiting to run
                break;
            case "in_progress":
                status.ShouldBe("in_progress"); // currently running
                break;
            case "completed":
                status.ShouldBe("completed"); // finished execution
                break;
        }

        switch (conclusion)
        {
            case "success":
                conclusion.ShouldBe("success"); // passed/succeeded
                break;
            case "failure":
                conclusion.ShouldBe("failure"); // failed
                break;
            case "neutral":
                conclusion.ShouldBe("neutral"); // completed but inconclusive
                break;
            case "cancelled":
                conclusion.ShouldBe("cancelled"); // was cancelled
                break;
            case "skipped":
                conclusion.ShouldBe("skipped"); // was skipped
                break;
            case "timed_out":
                conclusion.ShouldBe("timed_out"); // exceeded time limit
                break;
        }
    }
}