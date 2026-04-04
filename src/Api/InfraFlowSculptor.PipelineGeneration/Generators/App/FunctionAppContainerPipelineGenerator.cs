using System.Text;
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
            [$"{request.ResourceName}/ci.app-pipeline.yml"] = GenerateCiPipeline(request),
            [$"{request.ResourceName}/release.app-pipeline.yml"] = GenerateReleasePipeline(request),
        };

        return new AppPipelineGenerationResult { Files = files };
    }

    private static string GenerateCiPipeline(AppPipelineGenerationRequest request)
    {
        var sb = new StringBuilder();

        AppPipelineYamlHelper.AppendCiHeader(sb, request.ResourceName, request.ConfigName);
        AppPipelineYamlHelper.AppendContainerBuildStage(
            sb,
            request.ResourceName,
            request.DockerfilePath,
            request.DockerImageName,
            request.ContainerRegistryName,
            request.AgentPoolName);

        return sb.ToString();
    }

    private static string GenerateReleasePipeline(AppPipelineGenerationRequest request)
    {
        var sb = new StringBuilder();

        AppPipelineYamlHelper.AppendReleaseHeader(sb, request.ResourceName);
        AppPipelineYamlHelper.AppendContainerBuildStage(
            sb,
            request.ResourceName,
            request.DockerfilePath,
            request.DockerImageName,
            request.ContainerRegistryName,
            request.AgentPoolName);

        AppPipelineYamlHelper.AppendFunctionAppContainerDeployStages(sb, request);

        return sb.ToString();
    }
}
