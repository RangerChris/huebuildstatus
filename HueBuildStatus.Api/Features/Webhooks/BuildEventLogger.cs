using HueBuildStatus.Core.Features.Queue;
using HueBuildStatus.Core.Features.Hue;
using HueBuildStatus.Core.Features.Config;

namespace HueBuildStatus.Api.Features.Webhooks;

public class BuildEventLogger(ILogger<BuildEventLogger> logger, IHueLightService hueLightService, IAppConfiguration appConfiguration) : IBuildEventHandler
{
    public async Task HandleAsync(BuildEvent @event)
    {
        logger.LogInformation("Received build event: Action={Action}, Status={Status}, Conclusion={Conclusion}",
            @event.GithubAction, @event.Status, @event.Conclusion);

        if (string.IsNullOrEmpty(appConfiguration.LightName))
        {
            logger.LogWarning("LightName not configured, skipping light control");
            return;
        }

        var light = await hueLightService.GetLightByNameAsync(appConfiguration.LightName);
        if (light == null)
        {
            logger.LogWarning("Light '{LightName}' not found, skipping light control", appConfiguration.LightName);
            return;
        }

        if (@event.Status == "in_progress")
        {
            logger.LogInformation("Flashing light for build in progress");
                        await hueLightService.FlashLightAsync(light.Id, 5000);
        }
        else if (@event.Status == "completed")
        {
            if (@event.Conclusion == "success")
            {
                logger.LogInformation("Setting light to green for build success");
                                await hueLightService.SetLightColorAsync(light.Id, "green", 2000);
            }
            else if (@event.Conclusion == "failure")
            {
                logger.LogInformation("Setting light to red for build failure");
                                await hueLightService.SetLightColorAsync(light.Id, "red", 2000);
            }
        }
    }
}