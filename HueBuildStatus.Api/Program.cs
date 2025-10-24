using FastEndpoints;
using FastEndpoints.Swagger;
using HueBuildStatus.Api.Features.Hue;
using HueBuildStatus.Api.Features.Webhooks;
using HueBuildStatus.Core;
using HueBuildStatus.Core.Features.Queue;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

var telemetryEndpoint = builder.Configuration["OTLP_ENDPOINT_URL"];
var apiTelemetry = builder.Services.AddOpenTelemetry();

apiTelemetry.ConfigureResource(resource => resource
    .AddService(serviceName: builder.Environment.ApplicationName));

apiTelemetry.WithMetrics(metrics => metrics
    .AddAspNetCoreInstrumentation()
    .AddMeter("Microsoft.AspNetCore.Hosting")
    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
    .AddMeter("System.Net.Http")
    .AddMeter("System.Net.NameResolution")
);


apiTelemetry.WithTracing(tracing =>
{
    tracing.AddAspNetCoreInstrumentation();
    tracing.AddHttpClientInstrumentation();
    tracing.AddSource(TracingConstants.ActivitySourceName);
    if (telemetryEndpoint != null)
    {
        tracing.AddOtlpExporter(options => { options.Endpoint = new Uri(telemetryEndpoint); });
    }
});

// Register Hue feature services (single place)
builder.Services.AddHueFeature();

builder.Services.AddSingleton<EventQueue>();
builder.Services.AddSingleton<BuildEventLogger>();

builder.Services.AddFastEndpoints().SwaggerDocument(o =>
{
    o.DocumentSettings = s =>
    {
        s.Title = "Hue Build Status API";
        s.Description = """
                        This API provides endpoints to control Philips Hue lights for build status notifications and receive webhooks from CI/CD platforms.

                        ## Configuration
                        To use the Hue-related endpoints, configure the following settings in `appsettings.json`:
                        - `bridgeIp`: The IP address of your Hue Bridge (discover via GET /hue/discover)
                        - `bridgeKey`: The app key for authentication (obtain via POST /hue/register)
                        - `lightName`: The name of the light to control for build status

                        ## Usage
                        1. Discover and register your Hue Bridge using /hue/discover and /hue/register
                        2. Configure webhooks in your CI/CD platform (GitHub, Azure DevOps) to point to the appropriate webhook endpoints
                        3. Use the build status endpoints to manually control lights or let webhooks trigger them automatically
                        """;
        s.Version = "1.0";
    };
});
var app = builder.Build();

var queue = app.Services.GetRequiredService<EventQueue>();
var eventLogger = app.Services.GetRequiredService<BuildEventLogger>();
queue.Subscribe(eventLogger);

app.UseFastEndpoints().UseSwaggerGen();

app.Run();

public partial class Program
{
}