using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.PipelineGeneration.Generators.App;

/// <summary>
/// Generates CI/CD pipeline YAML for Azure Container App resources.
/// Container Apps are always deployed as containers (Docker build → ACR push → ACA revision update).
/// </summary>
public sealed class ContainerAppPipelineGenerator : IAppPipelineGenerator
{
    /// <inheritdoc />
    public string ResourceType => AzureResourceTypes.ContainerApp;

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
