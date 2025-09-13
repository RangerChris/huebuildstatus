namespace HueBuildStatus.Core.Features.Hue;

public interface IHueLightService
{
    Task<bool> SetLightOnOffAsync(string bridgeIp, string appKey, string lightId, bool on);
    Task<bool> SetLightColorAsync(string bridgeIp, string appKey, string lightId, string colorHex);
    Task<bool> SetLightBrightnessAsync(string bridgeIp, string appKey, string lightId, int brightness);
}