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

    /// <summary>
    /// Changes the color of a Hue light using a hex color string (e.g., "#FF0000" for red).
    /// </summary>
    /// <param name="bridgeIp">IP address of the Hue Bridge.</param>
    /// <param name="appKey">API key for authentication.</param>
    /// <param name="lightId">ID of the light to control.</param>
    /// <param name="colorHex">Hex color string (e.g., "#FF0000").</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> SetLightColorAsync(string bridgeIp, string appKey, string lightId, string colorHex);

    /// <summary>
    /// Changes the brightness of a Hue light (0-100).
    /// </summary>
    /// <param name="bridgeIp">IP address of the Hue Bridge.</param>
    /// <param name="appKey">API key for authentication.</param>
    /// <param name="lightId">ID of the light to control.</param>
    /// <param name="brightness">Brightness value (0-100).</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> SetLightBrightnessAsync(string bridgeIp, string appKey, string lightId, int brightness);
}
