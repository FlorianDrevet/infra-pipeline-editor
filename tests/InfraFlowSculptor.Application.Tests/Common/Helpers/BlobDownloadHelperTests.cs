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
            firstBucketName: "infra",
            secondBucketName: "app",
            legacyDefaultBucketName: "infra");

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
            firstBucketName: "infra",
            secondBucketName: "app",
            legacyDefaultBucketName: "infra");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }
}