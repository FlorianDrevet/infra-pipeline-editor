using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.Application.Common.Services;

/// <summary>
/// Creates typed app pipeline requests by loading the concrete compute resource and its optional container registry.
/// </summary>
public sealed class AppPipelineRequestFactory(
    IContainerAppRepository containerAppRepository,
    IWebAppRepository webAppRepository,
    IFunctionAppRepository functionAppRepository,
    IContainerRegistryRepository containerRegistryRepository)
    : IAppPipelineRequestFactory
{
    /// <inheritdoc />
    public async Task<AppPipelineGenerationRequest?> CreateAsync(
        AzureResourceId resourceId,
        string resourceType,
        CancellationToken cancellationToken = default)
    {
        return resourceType switch
        {
            AzureResourceTypes.ArmTypes.ContainerAppType => await CreateFromContainerAppAsync(resourceId, cancellationToken)
                .ConfigureAwait(false),
            AzureResourceTypes.ArmTypes.WebAppType => await CreateFromWebAppAsync(resourceId, cancellationToken)
                .ConfigureAwait(false),
            AzureResourceTypes.ArmTypes.FunctionAppType => await CreateFromFunctionAppAsync(resourceId, cancellationToken)
                .ConfigureAwait(false),
            _ => null,
        };
    }

    private async Task<AppPipelineGenerationRequest?> CreateFromContainerAppAsync(
        AzureResourceId resourceId,
        CancellationToken cancellationToken)
    {
        var containerApp = await containerAppRepository.GetByIdReadOnlyAsync(resourceId, cancellationToken).ConfigureAwait(false);
        if (containerApp is null)
            return null;

        var containerRegistryName = await ResolveContainerRegistryNameAsync(
            containerApp.ContainerRegistryId,
            cancellationToken).ConfigureAwait(false);

        return new AppPipelineGenerationRequest
        {
            ResourceName = containerApp.Name,
            ApplicationName = containerApp.ApplicationName,
            ResourceType = AzureResourceTypes.ContainerApp,
            DeploymentMode = DeploymentMode.DeploymentModeType.Container.ToString(),
            DockerfilePath = containerApp.DockerfilePath,
            DockerImageName = containerApp.DockerImageName,
            ContainerRegistryName = containerRegistryName,
            AcrAuthMode = containerApp.AcrAuthMode?.Value.ToString(),
            PromotionStrategy = AppPipelinePromotionStrategy.AcrImport,
            EnableSecurityScans = true,
            RunUnitTests = containerApp.PipelineStepOptions.RunUnitTests,
            TestCommand = containerApp.PipelineStepOptions.TestCommand,
            TestFramework = containerApp.PipelineStepOptions.TestFramework,
            TestResultsFormat = containerApp.PipelineStepOptions.TestResultsFormat,
            TestResultsPath = containerApp.PipelineStepOptions.TestResultsPath,
            PublishTestResults = containerApp.PipelineStepOptions.PublishTestResults,
            PublishCodeCoverage = containerApp.PipelineStepOptions.PublishCodeCoverage,
            CoverageTool = containerApp.PipelineStepOptions.CoverageTool,
            CoverageReportPath = containerApp.PipelineStepOptions.CoverageReportPath,
            RunSonarAnalysis = containerApp.PipelineStepOptions.RunSonarAnalysis,
            SonarProjectKey = containerApp.PipelineStepOptions.SonarProjectKey,
            SonarOrganization = containerApp.PipelineStepOptions.SonarOrganization,
            SonarServiceConnection = containerApp.PipelineStepOptions.SonarServiceConnection,
            RunLinting = containerApp.PipelineStepOptions.RunLinting,
            LintCommand = containerApp.PipelineStepOptions.LintCommand,
            RunDependencyScan = containerApp.PipelineStepOptions.RunDependencyScan,
            DependencyScanTool = containerApp.PipelineStepOptions.DependencyScanTool,
            RunBuildValidation = containerApp.PipelineStepOptions.RunBuildValidation,
            EnableDependencyCache = containerApp.PipelineStepOptions.EnableDependencyCache,
            RunSmokeTests = containerApp.PipelineStepOptions.RunSmokeTests,
            SmokeTestCommand = containerApp.PipelineStepOptions.SmokeTestCommand,
        };
    }

    private async Task<AppPipelineGenerationRequest?> CreateFromWebAppAsync(
        AzureResourceId resourceId,
        CancellationToken cancellationToken)
    {
        var webApp = await webAppRepository.GetByIdReadOnlyAsync(resourceId, cancellationToken).ConfigureAwait(false);
        if (webApp is null)
            return null;

        var containerRegistryName = await ResolveContainerRegistryNameAsync(
            webApp.ContainerRegistryId,
            cancellationToken).ConfigureAwait(false);

        return new AppPipelineGenerationRequest
        {
            ResourceName = webApp.Name,
            ApplicationName = webApp.ApplicationName,
            ResourceType = AzureResourceTypes.WebApp,
            DeploymentMode = webApp.DeploymentMode.Value.ToString(),
            DockerfilePath = webApp.DockerfilePath,
            SourceCodePath = webApp.SourceCodePath,
            BuildCommand = webApp.BuildCommand,
            DockerImageName = webApp.DockerImageName,
            ContainerRegistryName = containerRegistryName,
            AcrAuthMode = webApp.AcrAuthMode?.Value.ToString(),
            RuntimeStack = webApp.RuntimeStack.Value.ToString(),
            RuntimeVersion = webApp.RuntimeVersion,
            PromotionStrategy = AppPipelinePromotionStrategy.AcrImport,
            EnableSecurityScans = true,
            RunUnitTests = webApp.PipelineStepOptions.RunUnitTests,
            TestCommand = webApp.PipelineStepOptions.TestCommand,
            TestFramework = webApp.PipelineStepOptions.TestFramework,
            TestResultsFormat = webApp.PipelineStepOptions.TestResultsFormat,
            TestResultsPath = webApp.PipelineStepOptions.TestResultsPath,
            PublishTestResults = webApp.PipelineStepOptions.PublishTestResults,
            PublishCodeCoverage = webApp.PipelineStepOptions.PublishCodeCoverage,
            CoverageTool = webApp.PipelineStepOptions.CoverageTool,
            CoverageReportPath = webApp.PipelineStepOptions.CoverageReportPath,
            RunSonarAnalysis = webApp.PipelineStepOptions.RunSonarAnalysis,
            SonarProjectKey = webApp.PipelineStepOptions.SonarProjectKey,
            SonarOrganization = webApp.PipelineStepOptions.SonarOrganization,
            SonarServiceConnection = webApp.PipelineStepOptions.SonarServiceConnection,
            RunLinting = webApp.PipelineStepOptions.RunLinting,
            LintCommand = webApp.PipelineStepOptions.LintCommand,
            RunDependencyScan = webApp.PipelineStepOptions.RunDependencyScan,
            DependencyScanTool = webApp.PipelineStepOptions.DependencyScanTool,
            RunBuildValidation = webApp.PipelineStepOptions.RunBuildValidation,
            EnableDependencyCache = webApp.PipelineStepOptions.EnableDependencyCache,
            RunSmokeTests = webApp.PipelineStepOptions.RunSmokeTests,
            SmokeTestCommand = webApp.PipelineStepOptions.SmokeTestCommand,
        };
    }

    private async Task<AppPipelineGenerationRequest?> CreateFromFunctionAppAsync(
        AzureResourceId resourceId,
        CancellationToken cancellationToken)
    {
        var functionApp = await functionAppRepository.GetByIdReadOnlyAsync(resourceId, cancellationToken).ConfigureAwait(false);
        if (functionApp is null)
            return null;

        var containerRegistryName = await ResolveContainerRegistryNameAsync(
            functionApp.ContainerRegistryId,
            cancellationToken).ConfigureAwait(false);

        return new AppPipelineGenerationRequest
        {
            ResourceName = functionApp.Name,
            ApplicationName = functionApp.ApplicationName,
            ResourceType = AzureResourceTypes.FunctionApp,
            DeploymentMode = functionApp.DeploymentMode.Value.ToString(),
            DockerfilePath = functionApp.DockerfilePath,
            SourceCodePath = functionApp.SourceCodePath,
            BuildCommand = functionApp.BuildCommand,
            DockerImageName = functionApp.DockerImageName,
            ContainerRegistryName = containerRegistryName,
            AcrAuthMode = functionApp.AcrAuthMode?.Value.ToString(),
            RuntimeStack = functionApp.RuntimeStack.Value.ToString(),
            RuntimeVersion = functionApp.RuntimeVersion,
            PromotionStrategy = AppPipelinePromotionStrategy.AcrImport,
            EnableSecurityScans = true,
            RunUnitTests = functionApp.PipelineStepOptions.RunUnitTests,
            TestCommand = functionApp.PipelineStepOptions.TestCommand,
            TestFramework = functionApp.PipelineStepOptions.TestFramework,
            TestResultsFormat = functionApp.PipelineStepOptions.TestResultsFormat,
            TestResultsPath = functionApp.PipelineStepOptions.TestResultsPath,
            PublishTestResults = functionApp.PipelineStepOptions.PublishTestResults,
            PublishCodeCoverage = functionApp.PipelineStepOptions.PublishCodeCoverage,
            CoverageTool = functionApp.PipelineStepOptions.CoverageTool,
            CoverageReportPath = functionApp.PipelineStepOptions.CoverageReportPath,
            RunSonarAnalysis = functionApp.PipelineStepOptions.RunSonarAnalysis,
            SonarProjectKey = functionApp.PipelineStepOptions.SonarProjectKey,
            SonarOrganization = functionApp.PipelineStepOptions.SonarOrganization,
            SonarServiceConnection = functionApp.PipelineStepOptions.SonarServiceConnection,
            RunLinting = functionApp.PipelineStepOptions.RunLinting,
            LintCommand = functionApp.PipelineStepOptions.LintCommand,
            RunDependencyScan = functionApp.PipelineStepOptions.RunDependencyScan,
            DependencyScanTool = functionApp.PipelineStepOptions.DependencyScanTool,
            RunBuildValidation = functionApp.PipelineStepOptions.RunBuildValidation,
            EnableDependencyCache = functionApp.PipelineStepOptions.EnableDependencyCache,
            RunSmokeTests = functionApp.PipelineStepOptions.RunSmokeTests,
            SmokeTestCommand = functionApp.PipelineStepOptions.SmokeTestCommand,
        };
    }

    private async Task<string?> ResolveContainerRegistryNameAsync(
        AzureResourceId? containerRegistryId,
        CancellationToken cancellationToken)
    {
        if (containerRegistryId is null)
            return null;

        var containerRegistry = await containerRegistryRepository.GetByIdReadOnlyAsync(containerRegistryId, cancellationToken)
            .ConfigureAwait(false);

        return containerRegistry?.Name.Value;
    }
}
