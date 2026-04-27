using System.Text;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.PipelineGeneration.Generators.App;

using static PipelineGenerationEngine;

/// <summary>
/// Builds thin CI pipeline wrapper YAML files that reference shared templates via <c>extends:</c>.
/// Wrappers live under <c>.azuredevops/{configName}/apps/{appName}/</c> and reference templates under
/// <c>.azuredevops/Common/pipelines/</c>.
/// </summary>
internal static class AppCiPipelineBuilder
{
    private const string ContainerTemplatePath = "../../../Common/pipelines/app-ci-container.pipeline.yml";
    private const string CodeTemplatePath = "../../../Common/pipelines/app-ci-code.pipeline.yml";

    internal static string BuildContainerPipeline(AppPipelineGenerationRequest request)
    {
        var sb = new StringBuilder();
        var buildSourceEnvironment = AppNamingHelper.GetBuildSourceEnvironment(request);
        var buildSourceEnvKey = buildSourceEnvironment?.ShortName.ToLowerInvariant() ?? string.Empty;
        var imageRepository = AppNamingHelper.ResolveImageRepository(request);
        var imageTagPattern = AppNamingHelper.ResolveImageTagPattern(request);
        var dockerfilePath = request.DockerfilePath ?? "Dockerfile";
        var buildContext = request.SourceCodePath ?? ".";
        var acrAuthMode = request.AcrAuthMode ?? "ServiceConnection";
        var buildSourceEnvVariablesPath = string.IsNullOrWhiteSpace(buildSourceEnvKey)
            ? string.Empty
            : AppHeaderEmitter.GetEnvironmentVariablesPath(buildSourceEnvKey, request.IsMonoRepo);

        AppendCiHeader(sb, request);

        sb.AppendLine("extends:");
        sb.AppendLine($"  template: {ContainerTemplatePath}");
        sb.AppendLine("  parameters:");
        sb.AppendLine($"    resourceName: '{AppNamingHelper.EscapeForSingleQuotedYaml(request.ResourceName)}'");
        sb.AppendLine($"    configName: '{AppNamingHelper.EscapeForSingleQuotedYaml(request.ConfigName)}'");
        sb.AppendLine($"    resourceType: '{AppNamingHelper.EscapeForSingleQuotedYaml(request.ResourceType)}'");
        sb.AppendLine($"    imageRepository: '{AppNamingHelper.EscapeForSingleQuotedYaml(imageRepository)}'");
        sb.AppendLine($"    imageTagPattern: '{AppNamingHelper.EscapeForSingleQuotedYaml(imageTagPattern)}'");
        sb.AppendLine($"    dockerfilePath: '{AppNamingHelper.EscapeForSingleQuotedYaml(dockerfilePath)}'");
        sb.AppendLine($"    buildContext: '{AppNamingHelper.EscapeForSingleQuotedYaml(buildContext)}'");
        sb.AppendLine($"    containerRegistryName: '{AppNamingHelper.EscapeForSingleQuotedYaml(request.ContainerRegistryName ?? string.Empty)}'");
        sb.AppendLine($"    acrAuthMode: '{AppNamingHelper.EscapeForSingleQuotedYaml(acrAuthMode)}'");
        sb.AppendLine($"    enableSecurityScans: {(request.EnableSecurityScans ? "true" : "false")}");
        sb.AppendLine($"    promotionStrategy: '{request.PromotionStrategy}'");
        sb.AppendLine($"    buildSourceEnvVariablesPath: '{AppNamingHelper.EscapeForSingleQuotedYaml(buildSourceEnvVariablesPath)}'");
        AppendVariableGroupsParameter(sb, buildSourceEnvKey, request);
        AppendAgentPoolParameter(sb, request.AgentPoolName);

        return sb.ToString();
    }

    internal static string BuildCodePipeline(AppPipelineGenerationRequest request)
    {
        var sb = new StringBuilder();
        var imageRepository = AppNamingHelper.ResolveImageRepository(request);
        var imageTagPattern = AppNamingHelper.ResolveImageTagPattern(request);

        AppendCiHeader(sb, request);

        sb.AppendLine("extends:");
        sb.AppendLine($"  template: {CodeTemplatePath}");
        sb.AppendLine("  parameters:");
        sb.AppendLine($"    resourceName: '{AppNamingHelper.EscapeForSingleQuotedYaml(request.ResourceName)}'");
        sb.AppendLine($"    configName: '{AppNamingHelper.EscapeForSingleQuotedYaml(request.ConfigName)}'");
        sb.AppendLine($"    imageRepository: '{AppNamingHelper.EscapeForSingleQuotedYaml(imageRepository)}'");
        sb.AppendLine($"    imageTagPattern: '{AppNamingHelper.EscapeForSingleQuotedYaml(imageTagPattern)}'");
        sb.AppendLine($"    runtimeStack: '{AppNamingHelper.EscapeForSingleQuotedYaml(request.RuntimeStack?.ToUpperInvariant() ?? "DOTNETCORE")}'");
        sb.AppendLine($"    runtimeVersion: '{AppNamingHelper.EscapeForSingleQuotedYaml(request.RuntimeVersion ?? "8.0")}'");
        sb.AppendLine($"    sourcePath: '{AppNamingHelper.EscapeForSingleQuotedYaml(request.SourceCodePath ?? ".")}'");
        sb.AppendLine($"    testCommand: '{AppNamingHelper.EscapeForSingleQuotedYaml(request.TestCommand ?? string.Empty)}'");
        sb.AppendLine($"    buildCommand: '{AppNamingHelper.EscapeForSingleQuotedYaml(request.BuildCommand ?? string.Empty)}'");
        sb.AppendLine($"    promotionStrategy: '{request.PromotionStrategy}'");
        sb.AppendLine($"    resourceType: '{AppNamingHelper.EscapeForSingleQuotedYaml(request.ResourceType)}'");
        AppendAgentPoolParameter(sb, request.AgentPoolName);

        return sb.ToString();
    }

    private static void AppendCiHeader(StringBuilder sb, AppPipelineGenerationRequest request)
    {
        var configName = PathSanitizer.Sanitize(request.ConfigName);
        var resourceName = PathSanitizer.Sanitize(request.ResourceName);
        var appFolderName = PathSanitizer.Sanitize(request.ApplicationName ?? request.ResourceName);

        sb.AppendLine($"# CI Application Pipeline for {resourceName} — Auto-generated by InfraFlowSculptor");
        sb.AppendLine("name: $(Date:yyyyMMdd).$(Rev:r)");
        sb.AppendLine();
        sb.AppendLine("trigger:");
        sb.AppendLine("  branches:");
        sb.AppendLine("    include:");
        sb.AppendLine("      - main");
        sb.AppendLine("      - release/*");
        sb.AppendLine("  paths:");
        sb.AppendLine("    include:");
        sb.AppendLine($"      - {configName}/{resourceName}/*");
        sb.AppendLine("      - .azuredevops/Common/*");
        sb.AppendLine($"      - .azuredevops/{configName}/apps/{appFolderName}/*");
        sb.AppendLine();
    }

    private static void AppendVariableGroupsParameter(
        StringBuilder sb,
        string envKey,
        AppPipelineGenerationRequest request)
    {
        if (request.PipelineVariableGroups.Count == 0 || string.IsNullOrWhiteSpace(envKey))
        {
            sb.AppendLine("    variableGroups: []");
            return;
        }

        sb.AppendLine("    variableGroups:");
        foreach (var group in request.PipelineVariableGroups)
        {
            var resolvedName = group.GroupName.Replace("{env}", envKey, StringComparison.OrdinalIgnoreCase);
            sb.AppendLine($"      - '{AppNamingHelper.EscapeForSingleQuotedYaml(resolvedName)}'");
        }
    }

    private static void AppendAgentPoolParameter(StringBuilder sb, string? agentPoolName)
    {
        if (!string.IsNullOrWhiteSpace(agentPoolName))
        {
            sb.AppendLine($"    agentPoolName: '{AppNamingHelper.EscapeForSingleQuotedYaml(agentPoolName)}'");
        }
    }
}