using System.Text;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.PipelineGeneration.Generators.App;

/// <summary>
/// Generates CI/CD pipeline YAML for Azure Web App resources in Container deployment mode.
/// Builds a Docker image, pushes to ACR, and deploys to Web App using <c>AzureWebApp@1</c>.
/// </summary>
public sealed class WebAppContainerPipelineGenerator : IAppPipelineGenerator
{
    /// <inheritdoc />
    public string ResourceType => AzureResourceTypes.WebApp;

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

        AppPipelineYamlHelper.AppendCiHeader(sb, request.ResourceName, request.ConfigName, request.AgentPoolName);
        AppPipelineYamlHelper.AppendContainerBuildStage(sb, request);

        return sb.ToString();
    }

    private static string GenerateReleasePipeline(AppPipelineGenerationRequest request)
    {
        var sb = new StringBuilder();

        AppPipelineYamlHelper.AppendReleaseHeader(sb, request.ResourceName, request.AgentPoolName);
        AppPipelineYamlHelper.AppendContainerBuildStage(sb, request);

        AppPipelineYamlHelper.AppendWebAppContainerDeployStages(sb, request);

        return sb.ToString();
    }
}
