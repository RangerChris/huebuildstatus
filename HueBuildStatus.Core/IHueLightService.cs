namespace HueBuildStatus.Core;

public interface IHueLightService
{
    /// <summary>
    /// Turns a Hue light on or off.
    /// </summary>
    /// <param name="bridgeIp">IP address of the Hue Bridge.</param>
    /// <param name="appKey">API key for authentication.</param>
    /// <param name="lightId">ID of the light to control.</param>
    /// <param name="on">True to turn on, false to turn off.</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> SetLightOnOffAsync(string bridgeIp, string appKey, string lightId, bool on);
}
