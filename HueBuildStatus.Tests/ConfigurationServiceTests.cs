using HueBuildStatus.Core.Features.Config;
using Microsoft.Extensions.Configuration;
using Shouldly;

namespace HueBuildStatus.Tests;

public class ConfigurationServiceTests
{
    [Fact]
    public void AppConfiguration_ReturnsValues_WhenPresent()
    {
        var inMem = new Dictionary<string, string?>
        {
            ["bridgeIp"] = "10.0.0.5",
            ["bridgeKey"] = "abc123"
        };

        var config = new ConfigurationBuilder().AddInMemoryCollection(inMem).Build();
        var svc = new AppConfiguration(config);

        svc.BridgeIp.ShouldBe("10.0.0.5");
        svc.BridgeKey.ShouldBe("abc123");
    }

    [Fact]
    public void AppConfiguration_ReturnsNull_WhenMissing()
    {
        var config = new ConfigurationBuilder().Build();
        var svc = new AppConfiguration(config);

        svc.BridgeIp.ShouldBeNull();
        svc.BridgeKey.ShouldBeNull();
    }
}