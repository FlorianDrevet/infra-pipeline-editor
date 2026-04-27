namespace InfraFlowSculptor.PipelineGeneration.Tests.GoldenFileTests;

/// <summary>
/// Locks the exact set of file paths returned by
/// <see cref="AppPipelineGenerationEngine.GenerateSharedTemplates"/>. The set is consumed
/// by <c>InfraFlowSculptor.Application.Common.Generation.AppPipelineFileClassifier</c> as a
/// frozen routing table that decides whether a file goes to the Infrastructure repo or the
/// Application Code repo. Adding, removing, or renaming any of these paths breaks the
/// classifier and silently misroutes files in mono-repo / split-repo layouts.
/// </summary>
/// <remarks>
/// This test guards risk R2 of the pipeline refactor plan
/// (<c>docs/architecture/pipeline-refactoring/00-PLAN.md</c>).
/// Updating this list requires explicit user validation.
/// </remarks>
public sealed class AppPipelineSharedTemplatesStabilityTests
{
    private static readonly IReadOnlySet<string> ExpectedFrozenPaths = new HashSet<string>(StringComparer.Ordinal)
    {
        // ── Atomic step templates ──────────────────────────────────────
        ".azuredevops/steps/app-compute-release-tag.step.yml",
        ".azuredevops/steps/app-acr-login.step.yml",
        ".azuredevops/steps/app-docker-buildx-push.step.yml",
        ".azuredevops/steps/app-trivy-scan.step.yml",
        ".azuredevops/steps/app-syft-sbom.step.yml",
        ".azuredevops/steps/app-load-metadata.step.yml",
        ".azuredevops/steps/app-acr-promote.step.yml",
        ".azuredevops/steps/app-deploy-container.step.yml",
        ".azuredevops/steps/app-deploy-code.step.yml",
        ".azuredevops/steps/app-sdk-setup.step.yml",
        ".azuredevops/steps/app-build-code.step.yml",

        // ── Job templates ──────────────────────────────────────────────
        ".azuredevops/jobs/app-ci-container.job.yml",
        ".azuredevops/jobs/app-ci-code.job.yml",
        ".azuredevops/jobs/app-release-container.job.yml",
        ".azuredevops/jobs/app-release-code.job.yml",

        // ── Pipeline templates (extends: targets) ──────────────────────
        ".azuredevops/pipelines/app-ci-container.pipeline.yml",
        ".azuredevops/pipelines/app-ci-code.pipeline.yml",
        ".azuredevops/pipelines/app-release-container.pipeline.yml",
        ".azuredevops/pipelines/app-release-code.pipeline.yml",
    };

    [Fact]
    public void Given_AppPipelineSharedTemplates_When_GenerateAll_Then_KeysExactlyMatchClassifierFrozenSet()
    {
        // Arrange + Act
        var actualPaths = AppPipelineGenerationEngine.GenerateSharedTemplates().Keys
            .ToHashSet(StringComparer.Ordinal);

        // Assert
        actualPaths.Should().BeEquivalentTo(
            ExpectedFrozenPaths,
            "the AppPipelineFileClassifier in the Application layer relies on this exact set " +
            "to route files between Infrastructure and Application Code repos. Any change " +
            "(add / remove / rename) requires explicit user validation and a coordinated " +
            "update of the classifier and frontend i18n.");
    }

    [Fact]
    public void Given_AppPipelineSharedTemplates_When_GenerateAll_Then_ContainsExpectedFileCount()
    {
        // Act
        var actual = AppPipelineGenerationEngine.GenerateSharedTemplates();

        // Assert
        actual.Should().HaveCount(ExpectedFrozenPaths.Count,
            "the frozen path set has {0} entries; any drift breaks downstream routing", ExpectedFrozenPaths.Count);
    }
}
