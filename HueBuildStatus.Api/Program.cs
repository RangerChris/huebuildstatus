using FastEndpoints;
using FastEndpoints.Swagger;
using HueBuildStatus.Api.Features.Hue;

var builder = WebApplication.CreateBuilder(args);

// Register Hue feature services (single place)
builder.Services.AddHueFeature();

builder.Services.AddFastEndpoints().SwaggerDocument();
var app = builder.Build();

app.UseFastEndpoints().UseSwaggerGen();

app.Run();

public partial class Program
{
}