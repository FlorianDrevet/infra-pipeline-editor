using System.Text;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.PipelineGeneration.Generators.App;
/// <summary>
/// Builds thin CI pipeline wrapper YAML files that reference shared templates via <c>extends:</c>.
/// Wrappers live under <c>.azuredevops/{configName}/apps/{appName}/</c> and reference templates under
/// <c>.azuredevops/Common/pipelines/</c>.
/// </summary>
internal static class AppCiPipelineBuilder
{
    private const string ContainerTemplatePath = "../../../Common/pipelines/app-ci-container.pipeline.yml";
    private const string CodeTemplatePath = "../../../Common/pipelines/app-ci-code.pipeline.yml";
    private const string ContainerRegistryServiceConnectionParameterName = "containerRegistryServiceConnection";

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
        var containerRegistryServiceConnection = ResolveContainerRegistryServiceConnection(request, buildSourceEnvironment);
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
        AppendStringParam(sb, ContainerRegistryServiceConnectionParameterName, containerRegistryServiceConnection);
        sb.AppendLine($"    acrAuthMode: '{AppNamingHelper.EscapeForSingleQuotedYaml(acrAuthMode)}'");
        sb.AppendLine($"    enableSecurityScans: {(request.EnableSecurityScans ? "true" : "false")}");
        sb.AppendLine($"    promotionStrategy: '{request.PromotionStrategy}'");
        sb.AppendLine($"    buildSourceEnvVariablesPath: '{AppNamingHelper.EscapeForSingleQuotedYaml(buildSourceEnvVariablesPath)}'");
        AppendPipelineStepOptionsParameters(sb, request);
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
        AppendPipelineStepOptionsParameters(sb, request);
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
            var resolvedName = PipelineVariableGroupNameHelper.ResolveYamlScalar(group.GroupName, envKey);
            sb.AppendLine($"      - {resolvedName}");
        }
    }

    private static void AppendAgentPoolParameter(StringBuilder sb, string? agentPoolName)
    {
        if (!string.IsNullOrWhiteSpace(agentPoolName))
        {
            sb.AppendLine($"    agentPoolName: '{AppNamingHelper.EscapeForSingleQuotedYaml(agentPoolName)}'");
        }
    }

    private static string? ResolveContainerRegistryServiceConnection(
        AppPipelineGenerationRequest request,
        EnvironmentDefinition? buildSourceEnvironment)
    {
        if (buildSourceEnvironment is null || request.ContainerRegistryServiceConnections.Count == 0)
            return null;

        return request.ContainerRegistryServiceConnections
            .FirstOrDefault(connection => MatchesBuildSourceEnvironment(connection, buildSourceEnvironment))
            ?.ServiceConnectionName;
    }

    private static bool MatchesBuildSourceEnvironment(
        ContainerRegistryServiceConnectionDefinition connection,
        EnvironmentDefinition buildSourceEnvironment)
    {
        return string.Equals(connection.EnvironmentName, buildSourceEnvironment.ShortName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(connection.EnvironmentName, buildSourceEnvironment.Name, StringComparison.OrdinalIgnoreCase);
    }

    private static void AppendPipelineStepOptionsParameters(StringBuilder sb, AppPipelineGenerationRequest request)
    {
        AppendTestParameters(sb, request);
        AppendCoverageParameters(sb, request);
        AppendSonarParameters(sb, request);
        AppendLintingParameters(sb, request);
        AppendSecurityParameters(sb, request);
        AppendBoolParam(sb, "runBuildValidation", request.RunBuildValidation);
        AppendBoolParam(sb, "enableDependencyCache", request.EnableDependencyCache);
        AppendSmokeTestParameters(sb, request);
    }

    private static void AppendTestParameters(StringBuilder sb, AppPipelineGenerationRequest request)
    {
        if (!request.RunUnitTests)
            return;

        sb.AppendLine($"    runUnitTests: true");
        AppendStringParam(sb, "testCommand", request.TestCommand);
        AppendStringParam(sb, "testFramework", request.TestFramework);
        AppendStringParam(sb, "testResultsFormat", request.TestResultsFormat);
        AppendStringParam(sb, "testResultsPath", request.TestResultsPath);
        AppendBoolParam(sb, "publishTestResults", request.PublishTestResults);
    }

    private static void AppendCoverageParameters(StringBuilder sb, AppPipelineGenerationRequest request)
    {
        if (!request.PublishCodeCoverage)
            return;

        sb.AppendLine($"    publishCodeCoverage: true");
        AppendStringParam(sb, "coverageTool", request.CoverageTool);
        AppendStringParam(sb, "coverageReportPath", request.CoverageReportPath);
    }

    private static void AppendSonarParameters(StringBuilder sb, AppPipelineGenerationRequest request)
    {
        if (!request.RunSonarAnalysis)
            return;

        sb.AppendLine($"    runSonarAnalysis: true");
        AppendStringParam(sb, "sonarProjectKey", request.SonarProjectKey);
        AppendStringParam(sb, "sonarOrganization", request.SonarOrganization);
        AppendStringParam(sb, "sonarServiceConnection", request.SonarServiceConnection);
    }

    private static void AppendLintingParameters(StringBuilder sb, AppPipelineGenerationRequest request)
    {
        if (!request.RunLinting)
            return;

        sb.AppendLine($"    runLinting: true");
        AppendStringParam(sb, "lintCommand", request.LintCommand);
    }

    private static void AppendSecurityParameters(StringBuilder sb, AppPipelineGenerationRequest request)
    {
        if (!request.RunDependencyScan)
            return;

        sb.AppendLine($"    runDependencyScan: true");
        AppendStringParam(sb, "dependencyScanTool", request.DependencyScanTool);
    }

    private static void AppendSmokeTestParameters(StringBuilder sb, AppPipelineGenerationRequest request)
    {
        if (!request.RunSmokeTests)
            return;

        sb.AppendLine($"    runSmokeTests: true");
        AppendStringParam(sb, "smokeTestCommand", request.SmokeTestCommand);
    }

    private static void AppendStringParam(StringBuilder sb, string paramName, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            sb.AppendLine($"    {paramName}: '{AppNamingHelper.EscapeForSingleQuotedYaml(value)}'");
    }

    private static void AppendBoolParam(StringBuilder sb, string paramName, bool value)
    {
        if (value)
            sb.AppendLine($"    {paramName}: true");
    }
}
