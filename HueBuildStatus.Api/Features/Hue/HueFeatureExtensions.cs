// Add DI registrations for Hue feature services

using HueBuildStatus.Core.Features.Config;
using HueBuildStatus.Core.Features.Hue;

namespace HueBuildStatus.Api.Features.Hue;

public static class HueFeatureExtensions
{
    public static IServiceCollection AddHueFeature(this IServiceCollection services)
    {
        services.AddHttpClient();

        services.AddSingleton<IAppConfiguration>(sp => new AppConfiguration(sp.GetRequiredService<IConfiguration>()));

        services.AddScoped<IHueDiscoveryService, HueDiscoveryService>();
        services.AddScoped<IHueLightService, HueLightService>();

        return services;
    }
}