namespace HueBuildStatus.Core.Features.Hue;

public interface IHueLightService
{
    Task<string?> GetBridgeIpAsync(string? configuredBridgeIp = null);
}