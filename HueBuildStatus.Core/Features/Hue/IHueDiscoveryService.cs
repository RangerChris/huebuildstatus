namespace HueBuildStatus.Core.Features.Hue;

public interface IHueDiscoveryService
{
    Task<string?> DiscoverBridgeAsync();
    Task<string?> AuthenticateAsync(string bridgeIp, string deviceType);
}