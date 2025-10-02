namespace HueBuildStatus.Core.Features.Config;

public interface IAppConfiguration
{
    string? BridgeIp { get; }
    string? BridgeKey { get; }
}