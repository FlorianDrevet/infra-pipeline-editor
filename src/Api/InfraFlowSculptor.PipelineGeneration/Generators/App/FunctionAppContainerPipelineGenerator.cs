using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.PipelineGeneration.Generators.App;

/// <summary>
/// Generates CI/CD pipeline YAML for Azure Function App resources in Container deployment mode.
/// Builds a Docker image, pushes to ACR, and deploys to Function App using <c>AzureFunctionApp@2</c>.
/// </summary>
public sealed class FunctionAppContainerPipelineGenerator : IAppPipelineGenerator
{
    /// <inheritdoc />
    public string ResourceType => AzureResourceTypes.FunctionApp;

    /// <inheritdoc />
    public string DeploymentMode => "Container";

    /// <inheritdoc />
    public AppPipelineGenerationResult Generate(AppPipelineGenerationRequest request)
    {
        var files = new Dictionary<string, string>
        {
            ["ci.app-pipeline.yml"] = AppCiPipelineBuilder.BuildContainerPipeline(request),
            ["release.app-pipeline.yml"] = AppReleasePipelineBuilder.BuildContainerPipeline(request),
        };

        return new AppPipelineGenerationResult { Files = files };
    }
}
