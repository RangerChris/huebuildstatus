using Microsoft.Extensions.Configuration;

namespace HueBuildStatus.Core.Features.Config;

public class AppConfiguration : IAppConfiguration
{
    public AppConfiguration(IConfiguration configuration)
    {
        // Read from configuration (appsettings) or environment variables where appropriate.
        BridgeIp = configuration["bridgeIp"] ?? Environment.GetEnvironmentVariable("BRIDGE_IP");
        BridgeKey = configuration["bridgeKey"] ?? Environment.GetEnvironmentVariable("HUE_APP_KEY");
        LightName = configuration["lightName"] ?? Environment.GetEnvironmentVariable("LIGHT_NAME");
    }

    public string? BridgeIp { get; }
    public string? BridgeKey { get; }
    public string? LightName { get; }
}