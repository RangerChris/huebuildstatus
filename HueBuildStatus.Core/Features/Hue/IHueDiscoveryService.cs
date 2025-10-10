using HueApi.ColorConverters;
using HueApi.Models;
using HueApi.Models.Clip;

namespace HueBuildStatus.Core.Features.Hue;

public record LightSnapshot(Guid LightId, string JsonSnapshot);

public interface IHueDiscoveryService
{
    Task<string?> DiscoverBridgeAsync();
    Task<RegisterEntertainmentResult?> Register(string bridgeIp, string hueUser);
    Task SetColorOfLamp(Light light, RGBColor color);
    Task SetColorOfLamp(Guid lightId, RGBColor color);
    Task SetOnState(Guid lightId, bool on);
    Task PulsateAsync(Light light, RGBColor color, int cycles = 3, int periodMs = 1000, int steps = 20, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, string>?> GetAllLights();
    Task<LightSnapshot> CaptureLightSnapshotAsync(Guid lightId);
    Task RestoreLightSnapshotAsync(LightSnapshot snapshot, int transitionMs = 0);
    Task<string?> AuthenticateAsync(string bridgeIp, string deviceType);
}