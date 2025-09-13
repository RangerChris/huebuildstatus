using FastEndpoints;

public class HealthEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/health");
        AllowAnonymous();
        Description(x => x
            .WithSummary("Health check endpoint")
            .WithDescription("Returns 200 OK if the service is running."));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.OkAsync(cancellation: ct);
    }
}