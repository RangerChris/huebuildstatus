
using FastEndpoints;
using FastEndpoints.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();
builder.Services.AddSwaggerDoc();

var app = builder.Build();

app.UseFastEndpoints();
app.UseSwaggerGen();

app.Run();

// Health Endpoint
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
        await SendOkAsync(new { status = "ok" }, ct);
    }
}
