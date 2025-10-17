using System.Text.Json;
using FastEndpoints;

namespace HueBuildStatus.Api.Features.Webhooks;

public class GitHubPushEndpoint(ILogger<GitHubPushEndpoint> logger) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/webhooks/github");
        AllowAnonymous();
        Description(s => s.WithSummary("Receive GitHub webhook").WithDescription("Logs all received GitHub webhook information."));
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

    public override async Task HandleAsync(CancellationToken ct)
    {
        string? githubEvent = HttpContext.Request.Headers["X-GitHub-Event"];
        if (string.IsNullOrWhiteSpace(githubEvent))
        {
            logger.LogInformation("Missing 'X-GitHub-Event' header");
            await Send.ErrorsAsync(400, ct);
            return;
        }

        logger.LogInformation("X-GitHub-Event: {GitHubEvent}", githubEvent);

        string req;
        using (var reader = new StreamReader(HttpContext.Request.Body))
        {
            req = await reader.ReadToEndAsync(ct);
        }

        if (string.IsNullOrWhiteSpace(req))
        {
            logger.LogInformation("Received empty or null JSON payload");
            await Send.ErrorsAsync(400, ct);
            return;
        }

        try
        {
            using var doc = JsonDocument.Parse(req);
            if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.GetRawText() == "{}")
            {
                logger.LogInformation("Received empty JSON object");
                await Send.ErrorsAsync(400, ct);
                return;
            }

            var status = FindJsonProperty(doc.RootElement, "status");
            var conclusion = FindJsonProperty(doc.RootElement, "conclusion");

            logger.LogInformation("Status: {Status}, Conclusion: {Conclusion}", status, conclusion);

            await Send.OkAsync(new { status, conclusion }, cancellation: ct);
        }
        catch (JsonException)
        {
            logger.LogInformation("Received malformed JSON payload");
            await Send.ErrorsAsync(400, ct);
        }
    }
}