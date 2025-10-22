using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HueBuildStatus.Core.Features.Config;

public class AppConfiguration : IAppConfiguration
{
    public AppConfiguration(IConfiguration configuration, ILogger<AppConfiguration>? logger = null)
    {
        // Read from environment variables first, then configuration (appsettings)
        BridgeIp = Environment.GetEnvironmentVariable("bridgeIp") ?? configuration["bridgeIp"];
        BridgeKey = Environment.GetEnvironmentVariable("bridgeKey") ?? configuration["bridgeKey"];
        LightName = Environment.GetEnvironmentVariable("LightName") ?? configuration["lightName"];

        // Validate and log if any value is null or empty
        if (string.IsNullOrEmpty(BridgeIp))
        {
            logger?.LogWarning("BridgeIp is not configured. Set the 'bridgeIp' environment variable or 'bridgeIp' in appsettings.json");
        }
        if (string.IsNullOrEmpty(BridgeKey))
        {
            logger?.LogWarning("BridgeKey is not configured. Set the 'bridgeKey' environment variable or 'bridgeKey' in appsettings.json");
        }
        if (string.IsNullOrEmpty(LightName))
        {
            logger?.LogWarning("LightName is not configured. Set the 'LightName' environment variable or 'lightName' in appsettings.json");
        }
    }

    public string? BridgeIp { get; }
    public string? BridgeKey { get; }
    public string? LightName { get; }
}