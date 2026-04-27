using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.PipelineGeneration.Generators.App;

/// <summary>
/// Generates CI/CD pipeline YAML for Azure Web App resources in Code deployment mode.
/// Builds from source (restore → build → test → publish), deploys via <c>AzureWebApp@1</c>.
/// </summary>
public sealed class WebAppCodePipelineGenerator : IAppPipelineGenerator
{
    /// <inheritdoc />
    public string ResourceType => AzureResourceTypes.WebApp;

    /// <inheritdoc />
    public string DeploymentMode => "Code";

    /// <inheritdoc />
    public AppPipelineGenerationResult Generate(AppPipelineGenerationRequest request)
    {
        var files = new Dictionary<string, string>
        {
            ["ci.app-pipeline.yml"] = AppCiPipelineBuilder.BuildCodePipeline(request),
            ["release.app-pipeline.yml"] = AppReleasePipelineBuilder.BuildCodePipeline(request),
        };

        return new AppPipelineGenerationResult { Files = files };
    }
}
