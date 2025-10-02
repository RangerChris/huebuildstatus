// Add DI registrations for Hue feature services

using HueBuildStatus.Core.Features.Config;
using HueBuildStatus.Core.Features.Hue;

namespace HueBuildStatus.Api.Features.Hue;

public static class HueFeatureExtensions
{
    public static IServiceCollection AddHueFeature(this IServiceCollection services)
    {
        // Register HttpClient for Hue services
        services.AddHttpClient();

        // Register configuration wrapper
        services.AddSingleton<IAppConfiguration>(sp => new AppConfiguration(sp.GetRequiredService<IConfiguration>()));

        // Register Core services from Hue feature
        services.AddScoped<IHueDiscoveryService, HueDiscoveryService>();
        services.AddScoped<IHueLightService, HueLightService>();

        return services;
    }
}