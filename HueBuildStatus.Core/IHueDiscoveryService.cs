namespace HueBuildStatus.Core;

public interface IHueDiscoveryService
{
    /// <summary>
    ///     Discovers the IP address of the Philips Hue Bridge on the local network.
    /// </summary>
    /// <returns>IP address as a string, or null if not found.</returns>
    Task<string?> DiscoverBridgeAsync();
}
