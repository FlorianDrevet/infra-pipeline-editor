using System.Text;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.PipelineGeneration.Generators.App;

/// <summary>
/// Generates CI/CD pipeline YAML for Azure Function App resources in Code deployment mode.
/// Builds from source (restore → build → test → publish), deploys via <c>AzureFunctionApp@2</c>.
/// </summary>
public sealed class FunctionAppCodePipelineGenerator : IAppPipelineGenerator
{
    /// <inheritdoc />
    public string ResourceType => AzureResourceTypes.FunctionApp;

    /// <inheritdoc />
    public string DeploymentMode => "Code";

    /// <inheritdoc />
    public AppPipelineGenerationResult Generate(AppPipelineGenerationRequest request)
    {
        var files = new Dictionary<string, string>
        {
            ["ci.app-pipeline.yml"] = GenerateCiPipeline(request),
            ["release.app-pipeline.yml"] = GenerateReleasePipeline(request),
        };

        return new AppPipelineGenerationResult { Files = files };
    }

    private static string GenerateCiPipeline(AppPipelineGenerationRequest request)
    {
        var sb = new StringBuilder();

        AppPipelineYamlHelper.AppendCiHeader(sb, request.ResourceName, request.ConfigName, request.AgentPoolName);
        AppPipelineYamlHelper.AppendCodeBuildStage(
            sb,
            request.ResourceName,
            request.RuntimeStack,
            request.RuntimeVersion,
            request.SourceCodePath,
            request.BuildCommand);

        return sb.ToString();
    }

    private static string GenerateReleasePipeline(AppPipelineGenerationRequest request)
    {
        var sb = new StringBuilder();

        AppPipelineYamlHelper.AppendReleaseHeader(sb, request.ResourceName, request.AgentPoolName);
        AppPipelineYamlHelper.AppendCodeBuildStage(
            sb,
            request.ResourceName,
            request.RuntimeStack,
            request.RuntimeVersion,
            request.SourceCodePath,
            request.BuildCommand);

        AppPipelineYamlHelper.AppendFunctionAppCodeDeployStages(sb, request);

        return sb.ToString();
    }
}
