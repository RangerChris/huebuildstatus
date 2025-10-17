using FastEndpoints;
using FastEndpoints.Swagger;
using HueBuildStatus.Api.Features.Hue;
using HueBuildStatus.Api.Features.Webhooks;
using HueBuildStatus.Core.Features.Queue;

var builder = WebApplication.CreateBuilder(args);

// Register Hue feature services (single place)
builder.Services.AddHueFeature();

builder.Services.AddSingleton<EventQueue>();
builder.Services.AddSingleton<BuildEventLogger>();

builder.Services.AddFastEndpoints().SwaggerDocument();
var app = builder.Build();

var queue = app.Services.GetRequiredService<EventQueue>();
var eventLogger = app.Services.GetRequiredService<BuildEventLogger>();
queue.Subscribe(eventLogger);

app.UseFastEndpoints().UseSwaggerGen();

app.Run();

public partial class Program
{
}