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
            ["ci.app-pipeline.yml"] = AppCiPipelineBuilder.BuildContainerPipeline(request),
            ["release.app-pipeline.yml"] = AppReleasePipelineBuilder.BuildContainerPipeline(request),
        };

        return new AppPipelineGenerationResult { Files = files };
    }
}
