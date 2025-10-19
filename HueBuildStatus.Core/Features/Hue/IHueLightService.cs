namespace HueBuildStatus.Core.Features.Hue;

public interface IHueLightService
{
    Task<string?> GetBridgeIpAsync(string? configuredBridgeIp = null);

    Task<string?> RegisterBridgeAsync(string bridgeIp);

    Task<Dictionary<Guid, string>> GetAllLightsAsync();

    Task<BuildLightInfo?> GetLightByNameAsync(string name);

    Task<LightSnapshot?> CaptureLightSnapshotAsync(Guid lightId);

    // Show a color (red, green, yellow) on the specified light for the given duration (ms), then restore the previous state
    Task<bool> SetLightColorAsync(Guid lightId, string colorName, int showDurationMs = 2000);

    // Flash the specified light (on/off/on/off) for the given duration (ms), then restore the previous state
    Task<bool> FlashLightAsync(Guid lightId, int durationMs = 5000);
}