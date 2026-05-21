using InfraFlowSculptor.PipelineGeneration.Generators.App.SharedTemplates;

namespace InfraFlowSculptor.PipelineGeneration.Generators.App;

/// <summary>
/// Generates the modular Azure DevOps YAML template tree for application CI/CD pipelines.
/// Output is split across <c>.azuredevops/{pipelines,jobs,steps}/</c> following the same
/// layout used for infrastructure pipelines.
/// </summary>
internal static class AppPipelineTemplatesGenerator
{
    /// <summary>
    /// Generates all shared application pipeline template files.
    /// Returned paths are relative to the repository root and prefixed by <c>.azuredevops/</c>.
    /// </summary>
    internal static IReadOnlyDictionary<string, string> GenerateAll()
    {
        return new Dictionary<string, string>
        {
            [".azuredevops/steps/app-compute-release-tag.step.yml"] = AppPipelineStepTemplates.ComputeReleaseTagStep,
            [".azuredevops/steps/app-acr-login.step.yml"] = AppPipelineStepTemplates.AcrLoginStep,
            [".azuredevops/steps/app-docker-buildx-push.step.yml"] = AppPipelineStepTemplates.DockerBuildxPushStep,
            [".azuredevops/steps/app-docker-buildx-validate.step.yml"] = AppPipelineStepTemplates.DockerBuildxValidateStep,
            [".azuredevops/steps/app-trivy-scan.step.yml"] = AppPipelineStepTemplates.TrivyScanStep,
            [".azuredevops/steps/app-syft-sbom.step.yml"] = AppPipelineStepTemplates.SyftSbomStep,
            [".azuredevops/steps/app-load-metadata.step.yml"] = AppPipelineStepTemplates.LoadMetadataStep,
            [".azuredevops/steps/app-acr-promote.step.yml"] = AppPipelineStepTemplates.AcrPromoteStep,
            [".azuredevops/steps/app-deploy-container.step.yml"] = AppPipelineStepTemplates.DeployContainerStep,
            [".azuredevops/steps/app-deploy-code.step.yml"] = AppPipelineStepTemplates.DeployCodeStep,
            [".azuredevops/steps/app-sdk-setup.step.yml"] = AppPipelineStepTemplates.SdkSetupStep,
            [".azuredevops/steps/app-build-code.step.yml"] = AppPipelineStepTemplates.BuildCodeStep,
            [".azuredevops/steps/app-dependency-scan.step.yml"] = AppPipelineStepTemplates.DependencyScanStep,
            [".azuredevops/steps/app-dependency-cache.step.yml"] = AppPipelineStepTemplates.DependencyCacheStep,
            [".azuredevops/steps/app-smoke-test.step.yml"] = AppPipelineStepTemplates.SmokeTestStep,
            [".azuredevops/steps/app-sonar-analysis.step.yml"] = AppPipelineStepTemplates.SonarAnalysisStep,
            [".azuredevops/steps/app-linting.step.yml"] = AppPipelineStepTemplates.LintingStep,
            [".azuredevops/jobs/app-ci-container.job.yml"] = AppPipelineJobTemplates.CiContainerJob,
            [".azuredevops/jobs/app-ci-code.job.yml"] = AppPipelineJobTemplates.CiCodeJob,
            [".azuredevops/jobs/app-pr-container.job.yml"] = AppPipelineJobTemplates.PrContainerJob,
            [".azuredevops/jobs/app-pr-code.job.yml"] = AppPipelineJobTemplates.PrCodeJob,
            [".azuredevops/jobs/app-release-container.job.yml"] = AppPipelineJobTemplates.ReleaseContainerJob,
            [".azuredevops/jobs/app-release-code.job.yml"] = AppPipelineJobTemplates.ReleaseCodeJob,
            [".azuredevops/pipelines/app-ci-container.pipeline.yml"] = AppPipelinePipelineTemplates.CiContainerPipeline,
            [".azuredevops/pipelines/app-ci-code.pipeline.yml"] = AppPipelinePipelineTemplates.CiCodePipeline,
            [".azuredevops/pipelines/app-pr-container.pipeline.yml"] = AppPipelinePipelineTemplates.PrContainerPipeline,
            [".azuredevops/pipelines/app-pr-code.pipeline.yml"] = AppPipelinePipelineTemplates.PrCodePipeline,
            [".azuredevops/pipelines/app-release-container.pipeline.yml"] = AppPipelinePipelineTemplates.ReleaseContainerPipeline,
            [".azuredevops/pipelines/app-release-code.pipeline.yml"] = AppPipelinePipelineTemplates.ReleaseCodePipeline,
        };
    }
}
