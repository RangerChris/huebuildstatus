namespace HueBuildStatus.Core;

public interface IHueDiscoveryService
{
    /// <summary>
    ///     Discovers the IP address of the Philips Hue Bridge on the local network.
    /// </summary>
    /// <returns>IP address as a string, or null if not found.</returns>
    Task<string?> DiscoverBridgeAsync();

    /// <summary>
    ///     Authenticates with the Hue Bridge and generates a new appkey.
    /// </summary>
    /// <param name="bridgeIp">The IP address of the Hue Bridge.</param>
    /// <param name="deviceType">A name for the application/device.</param>
    /// <returns>The appkey as a string, or null if authentication fails.</returns>
    Task<string?> AuthenticateAsync(string bridgeIp, string deviceType);
}