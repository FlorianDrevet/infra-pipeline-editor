using System.IO.Compression;
using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.Common.Errors;
using NSubstitute;

namespace InfraFlowSculptor.Application.Tests.Common.Helpers;

public sealed class BlobDownloadHelperTests
{
    private readonly IBlobService _blobService;

    public BlobDownloadHelperTests()
    {
        _blobService = Substitute.For<IBlobService>();
    }

    [Fact]
    public async Task Given_MultipleGenerationFolders_When_GetLatestBlobFolderAsync_Then_ReturnsLatestRelativePathsAndTimestampWithoutDownloadingAsync()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        _blobService.ListBlobsAsync($"pipeline/project/{entityId}/")
            .Returns(
            [
                $"pipeline/project/{entityId}/20260512090000/infra/.azuredevops/main.yml",
                $"pipeline/project/{entityId}/20260512090000/app/.azuredevops/apps/api/ci.yml",
                $"pipeline/project/{entityId}/20260511090000/.azuredevops/legacy.yml",
            ]);

        // Act
        var result = await BlobDownloadHelper.GetLatestBlobFolderAsync(
            _blobService,
            blobPrefix: $"pipeline/project/{entityId}/",
            prefixSegmentCount: 4);

        // Assert
        result.Should().NotBeNull();
        result!.Timestamp.Should().Be("20260512090000");
        result.RelativePaths.Should().BeEquivalentTo(
        [
            "infra/.azuredevops/main.yml",
            "app/.azuredevops/apps/api/ci.yml",
        ]);
        await _blobService.DidNotReceive().DownloadContentAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Given_LatestSplitArtifacts_When_GetLatestDualBucketBlobFilesAsync_Then_ReturnsSeparatedBucketsAsync()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        _blobService.ListBlobsAsync($"pipeline/project/{entityId}/")
            .Returns(
            [
                $"pipeline/project/{entityId}/20260512090000/infra/.azuredevops/main.yml",
                $"pipeline/project/{entityId}/20260512090000/app/.azuredevops/apps/api/ci.yml",
                $"pipeline/project/{entityId}/20260512090000/.azuredevops/legacy.yml",
                $"pipeline/project/{entityId}/20260511090000/infra/.azuredevops/old.yml",
            ]);
        _blobService.DownloadContentAsync($"pipeline/project/{entityId}/20260512090000/infra/.azuredevops/main.yml")
            .Returns("infra-main");
        _blobService.DownloadContentAsync($"pipeline/project/{entityId}/20260512090000/app/.azuredevops/apps/api/ci.yml")
            .Returns("app-ci");
        _blobService.DownloadContentAsync($"pipeline/project/{entityId}/20260512090000/.azuredevops/legacy.yml")
            .Returns("legacy");

        // Act
        var result = await BlobDownloadHelper.GetLatestDualBucketBlobFilesAsync(
            _blobService,
            blobPrefix: $"pipeline/project/{entityId}/",
            prefixSegmentCount: 4,
            notFoundErrorFactory: Errors.Project.PipelineFilesNotFoundError,
            entityId,
            new BlobDownloadHelper.DualBucketBlobFilesOptions(
                FirstBucketName: "infra",
                SecondBucketName: "app",
                LegacyDefaultBucketName: "infra"));

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.First.Should().ContainKey(".azuredevops/main.yml").WhoseValue.Should().Be("infra-main");
        result.Value.First.Should().ContainKey(".azuredevops/legacy.yml").WhoseValue.Should().Be("legacy");
        result.Value.Second.Should().ContainKey(".azuredevops/apps/api/ci.yml").WhoseValue.Should().Be("app-ci");
    }

    [Fact]
    public async Task Given_NoArtifacts_When_GetLatestDualBucketBlobFilesAsync_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        _blobService.ListBlobsAsync($"pipeline/project/{entityId}/")
            .Returns([]);

        // Act
        var result = await BlobDownloadHelper.GetLatestDualBucketBlobFilesAsync(
            _blobService,
            blobPrefix: $"pipeline/project/{entityId}/",
            prefixSegmentCount: 4,
            notFoundErrorFactory: Errors.Project.PipelineFilesNotFoundError,
            entityId,
            new BlobDownloadHelper.DualBucketBlobFilesOptions(
                FirstBucketName: "infra",
                SecondBucketName: "app",
                LegacyDefaultBucketName: "infra"));

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_LatestArtifacts_When_DownloadLatestBlobsAsZipAsync_Then_ArchivesOnlyLatestFolderAsync()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        _blobService.ListBlobsAsync($"bicep/{entityId}/")
            .Returns(
            [
                $"bicep/{entityId}/20260512090000/main.bicep",
                $"bicep/{entityId}/20260512090000/modules/app.bicep",
                $"bicep/{entityId}/20260511090000/old.bicep",
            ]);
        _blobService.DownloadContentAsync($"bicep/{entityId}/20260512090000/main.bicep")
            .Returns("main-content");
        _blobService.DownloadContentAsync($"bicep/{entityId}/20260512090000/modules/app.bicep")
            .Returns("module-content");

        // Act
        var result = await BlobDownloadHelper.DownloadLatestBlobsAsZipAsync(
            _blobService,
            blobPrefix: $"bicep/{entityId}/",
            prefixSegmentCount: 3,
            fileNameSuffix: "bicep",
            notFoundErrorFactory: Errors.InfrastructureConfig.BicepFilesNotFoundError,
            entityId,
            CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.FileName.Should().Be($"project-bicep-{entityId:N}.zip");

        using var archive = new ZipArchive(new MemoryStream(result.Value.Data), ZipArchiveMode.Read);
        archive.Entries.Should().ContainSingle(entry => entry.FullName == "main.bicep");
        archive.Entries.Should().ContainSingle(entry => entry.FullName == "modules/app.bicep");
        archive.Entries.Should().NotContain(entry => entry.FullName == "old.bicep");
    }

    [Fact]
    public async Task Given_LatestArtifacts_When_GetLatestBlobContentAsync_Then_ReturnsRequestedFileFromLatestFolderAsync()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        _blobService.ListBlobsAsync($"bicep/project/{entityId}/")
            .Returns(
            [
                $"bicep/project/{entityId}/20260512090000/main.bicep",
                $"bicep/project/{entityId}/20260511090000/old.bicep",
            ]);
        _blobService.DownloadContentAsync($"bicep/project/{entityId}/20260512090000/main.bicep")
            .Returns("latest-main");

        // Act
        var result = await BlobDownloadHelper.GetLatestBlobContentAsync(
            _blobService,
            blobPrefix: $"bicep/project/{entityId}/",
            prefixSegmentCount: 4,
            notFoundErrorFactory: Errors.Project.BicepFilesNotFoundError,
            entityId,
            options: new BlobDownloadHelper.LatestBlobContentOptions(
                Errors.Project.BicepFileNotFoundError,
                "main.bicep",
                ["main.bicep"]));

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be("latest-main");
    }

    [Fact]
    public async Task Given_SplitPipelineArtifacts_When_GetLatestBlobContentAsync_Then_FallsBackToInfraPathAsync()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        _blobService.ListBlobsAsync($"pipeline/project/{entityId}/")
            .Returns(
            [
                $"pipeline/project/{entityId}/20260512090000/infra/.azuredevops/main.yml",
                $"pipeline/project/{entityId}/20260511090000/.azuredevops/old.yml",
            ]);
        _blobService.DownloadContentAsync($"pipeline/project/{entityId}/20260512090000/infra/.azuredevops/main.yml")
            .Returns("infra-pipeline");

        // Act
        var result = await BlobDownloadHelper.GetLatestBlobContentAsync(
            _blobService,
            blobPrefix: $"pipeline/project/{entityId}/",
            prefixSegmentCount: 4,
            notFoundErrorFactory: Errors.Project.PipelineFilesNotFoundError,
            entityId,
            options: new BlobDownloadHelper.LatestBlobContentOptions(
                Errors.Project.PipelineFileNotFoundError,
                ".azuredevops/main.yml",
                [
                    ".azuredevops/main.yml",
                    "infra/.azuredevops/main.yml",
                    "app/.azuredevops/main.yml",
                ]));

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be("infra-pipeline");
    }

    [Fact]
    public async Task Given_ListIsCanceled_When_GetLatestBlobFilesAsync_Then_PropagatesCancellationAsync()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var cancellationToken = new CancellationToken(canceled: true);
        var prefix = $"bicep/project/{entityId}/";
        _blobService.ListBlobsAsync(prefix, cancellationToken)
            .Returns(Task.FromCanceled<IReadOnlyList<string>>(cancellationToken));

        // Act
        Func<Task> act = async () => await BlobDownloadHelper.GetLatestBlobFilesAsync(
            _blobService,
            blobPrefix: prefix,
            prefixSegmentCount: 4,
            notFoundErrorFactory: Errors.Project.BicepFilesNotFoundError,
            entityId,
            options: new BlobDownloadHelper.LatestBlobFilesOptions(
                CancellationToken: cancellationToken));

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
