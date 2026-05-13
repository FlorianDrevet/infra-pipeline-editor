using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common.Storage;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.PipelineGeneration.Models;
using NSubstitute;

namespace InfraFlowSculptor.Application.Tests.Projects.Common.Storage;

public sealed class MonoRepoBlobUploadOrchestratorTests
{
    private readonly IBlobService _blobService;
    private readonly IMonoRepoBlobUploadOrchestrator _sut;

    public MonoRepoBlobUploadOrchestratorTests()
    {
        _blobService = Substitute.For<IBlobService>();
        _blobService.UploadContentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo =>
                new Uri($"https://example.test/{Uri.EscapeDataString(callInfo.ArgAt<string>(0))}"));

        _sut = new MonoRepoBlobUploadOrchestrator(_blobService);
    }

    [Fact]
    public async Task Given_BicepArtifacts_When_UploadBicepAsync_Then_MapsCommonAndConfigUrisAsync()
    {
        // Arrange
        const string prefix = "bicep/project/project-123/20260513120000";
        var generationResult = new MonoRepoGenerationResult
        {
            CommonFiles = new Dictionary<string, string>
            {
                ["types.bicep"] = "shared-types",
            },
            ConfigFiles = new Dictionary<string, IReadOnlyDictionary<string, string>>
            {
                ["core"] = new Dictionary<string, string>
                {
                    ["main.bicep"] = "resource main",
                },
            },
        };

        // Act
        var result = await _sut.UploadBicepAsync(prefix, generationResult, CancellationToken.None);

        // Assert
        result.CommonFileUris.Keys.Should().Contain("Common/types.bicep");
        result.ConfigFileUris["core"].Keys.Should().Contain("main.bicep");
        await _blobService.Received(1)
            .UploadContentAsync($"{prefix}/Common/types.bicep", "shared-types", "text/plain");
        await _blobService.Received(1)
            .UploadContentAsync($"{prefix}/core/main.bicep", "resource main", "text/plain");
    }

    [Fact]
    public async Task Given_AppWrapperPipelines_When_UploadPipelineAsync_Then_UploadsAppAndInfraBucketsAsync()
    {
        // Arrange
        const string prefix = "pipeline/project/project-123/20260513120000";
        var generationResult = new MonoRepoPipelineResult
        {
            CommonFiles = new Dictionary<string, string>
            {
                ["pipelines/ci.pipeline.yml"] = "infra-common",
            },
            ConfigFiles = new Dictionary<string, IReadOnlyDictionary<string, string>>
            {
                ["core"] = new Dictionary<string, string>
                {
                    ["release.pipeline.yml"] = "infra-core",
                    ["apps/api/ci.pipeline.yml"] = "app-core",
                },
            },
        };

        // Act
        var result = await _sut.UploadPipelineAsync(prefix, generationResult, CancellationToken.None);

        // Assert
        result.InfraCommonFileUris.Keys.Should().Contain(".azuredevops/Common/pipelines/ci.pipeline.yml");
        result.AppCommonFileUris.Should().NotBeEmpty();
        result.InfraConfigFileUris["core"].Keys.Should().Contain(".azuredevops/core/release.pipeline.yml");
        result.AppConfigFileUris["core"].Keys.Should().Contain(".azuredevops/core/apps/api/ci.pipeline.yml");
        result.CommonFileUris.Should().ContainKey(".azuredevops/Common/pipelines/ci.pipeline.yml");
        result.ConfigFileUris["core"].Should().ContainKey(".azuredevops/core/apps/api/ci.pipeline.yml");
        await _blobService.Received()
            .UploadContentAsync(
                Arg.Is<string>(path => path.StartsWith($"{prefix}/app/.azuredevops/Common/", StringComparison.Ordinal)),
                Arg.Any<string>(),
                "text/plain");
    }

    [Fact]
    public async Task Given_InfraOnlyPipelines_When_UploadPipelineAsync_Then_SkipsSharedAppTemplatesAsync()
    {
        // Arrange
        const string prefix = "pipeline/project/project-123/20260513120000";
        var generationResult = new MonoRepoPipelineResult
        {
            CommonFiles = new Dictionary<string, string>
            {
                ["pipelines/ci.pipeline.yml"] = "infra-common",
            },
            ConfigFiles = new Dictionary<string, IReadOnlyDictionary<string, string>>
            {
                ["core"] = new Dictionary<string, string>
                {
                    ["release.pipeline.yml"] = "infra-core",
                },
            },
        };

        // Act
        var result = await _sut.UploadPipelineAsync(prefix, generationResult, CancellationToken.None);

        // Assert
        result.AppCommonFileUris.Should().BeEmpty();
        await _blobService.DidNotReceive()
            .UploadContentAsync(
                Arg.Is<string>(path => path.StartsWith($"{prefix}/app/.azuredevops/Common/", StringComparison.Ordinal)),
                Arg.Any<string>(),
                Arg.Any<string>());
    }
}