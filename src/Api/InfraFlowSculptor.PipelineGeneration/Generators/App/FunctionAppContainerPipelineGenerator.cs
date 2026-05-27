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
            [AppPipelineFileNames.Ci] = AppCiPipelineBuilder.BuildContainerPipeline(request),
            [AppPipelineFileNames.Pr] = AppPrPipelineBuilder.BuildContainerPipeline(request),
            [AppPipelineFileNames.Release] = AppReleasePipelineBuilder.BuildContainerPipeline(request),
        };

        return new AppPipelineGenerationResult { Files = files };
    }
}
