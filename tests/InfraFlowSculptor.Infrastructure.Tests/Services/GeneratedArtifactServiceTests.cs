using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Infrastructure.Services;
using NSubstitute;
using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Services;

public sealed class GeneratedArtifactServiceTests
{
    private readonly IBlobService _blobService = Substitute.For<IBlobService>();
    private readonly GeneratedArtifactService _sut;

    public GeneratedArtifactServiceTests()
    {
        _sut = new GeneratedArtifactService(_blobService);
    }

    // ──────────────────────────────────────────────────
    //  DownloadLatestAsync
    // ──────────────────────────────────────────────────

    [Fact]
    public async Task Given_NoBlobsExist_When_DownloadLatestAsync_Then_ReturnsNullAsync()
    {
        // Arrange
        var configId = Guid.NewGuid();
        _blobService.ListBlobsAsync($"bicep/{configId}/").Returns([]);

        // Act
        var result = await _sut.DownloadLatestAsync("bicep", configId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_MultipleBlobVersions_When_DownloadLatestAsync_Then_SelectsLatestTimestampAsync()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var prefix = $"bicep/{configId}/";
        var oldBlob = $"bicep/{configId}/20260101-120000/main.bicep";
        var newBlob = $"bicep/{configId}/20260517-140000/main.bicep";

        _blobService.ListBlobsAsync(prefix).Returns([oldBlob, newBlob]);
        _blobService.DownloadContentAsync(newBlob).Returns("resource {}");

        // Act
        var result = await _sut.DownloadLatestAsync("bicep", configId);

        // Assert
        result.Should().NotBeNull();
        result!.Value.FileName.Should().Be($"bicep-{configId:N}.zip");
        result.Value.ZipContent.Should().NotBeEmpty();

        await _blobService.DidNotReceive().DownloadContentAsync(oldBlob);
    }

    [Fact]
    public async Task Given_BlobContentIsNull_When_DownloadLatestAsync_Then_SkipsBlobInZipAsync()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var blob1 = $"bicep/{configId}/20260517-140000/main.bicep";
        var blob2 = $"bicep/{configId}/20260517-140000/modules/storage.bicep";

        _blobService.ListBlobsAsync($"bicep/{configId}/").Returns([blob1, blob2]);
        _blobService.DownloadContentAsync(blob1).Returns("resource {}");
        _blobService.DownloadContentAsync(blob2).Returns((string?)null);

        // Act
        var result = await _sut.DownloadLatestAsync("bicep", configId);

        // Assert
        result.Should().NotBeNull();
        result!.Value.ZipContent.Should().NotBeEmpty();
    }

    // ──────────────────────────────────────────────────
    //  GetFileContentAsync
    // ──────────────────────────────────────────────────

    [Fact]
    public async Task Given_NoBlobsExist_When_GetFileContentAsync_Then_ReturnsNullAsync()
    {
        // Arrange
        var configId = Guid.NewGuid();
        _blobService.ListBlobsAsync($"bicep/{configId}/").Returns([]);

        // Act
        var result = await _sut.GetFileContentAsync("bicep", configId, "main.bicep");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_BlobsExist_When_GetFileContentAsync_Then_ReturnsContentFromLatestVersionAsync()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var oldBlob = $"bicep/{configId}/20260101-120000/main.bicep";
        var newBlob = $"bicep/{configId}/20260517-140000/main.bicep";
        var expectedBlobPath = $"bicep/{configId}/20260517-140000/main.bicep";

        _blobService.ListBlobsAsync($"bicep/{configId}/").Returns([oldBlob, newBlob]);
        _blobService.DownloadContentAsync(expectedBlobPath).Returns("resource vnet 'Microsoft.Network/virtualNetworks@2024-01-01' = {}");

        // Act
        var result = await _sut.GetFileContentAsync("bicep", configId, "main.bicep");

        // Assert
        result.Should().Be("resource vnet 'Microsoft.Network/virtualNetworks@2024-01-01' = {}");
    }

    // ──────────────────────────────────────────────────
    //  GetLatestFilesAsync
    // ──────────────────────────────────────────────────

    [Fact]
    public async Task Given_NoBlobsExist_When_GetLatestFilesAsync_Then_ReturnsNullAsync()
    {
        // Arrange
        var configId = Guid.NewGuid();
        _blobService.ListBlobsAsync($"bicep/{configId}/").Returns([]);

        // Act
        var result = await _sut.GetLatestFilesAsync("bicep", configId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_AllBlobContentsNull_When_GetLatestFilesAsync_Then_ReturnsNullAsync()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var blob = $"bicep/{configId}/20260517-140000/main.bicep";

        _blobService.ListBlobsAsync($"bicep/{configId}/").Returns([blob]);
        _blobService.DownloadContentAsync(blob).Returns((string?)null);

        // Act
        var result = await _sut.GetLatestFilesAsync("bicep", configId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_MultipleFilesInLatestVersion_When_GetLatestFilesAsync_Then_ReturnsDictionaryWithRelativePathsAsync()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var blob1 = $"bicep/{configId}/20260517-140000/main.bicep";
        var blob2 = $"bicep/{configId}/20260517-140000/modules/storage.bicep";

        _blobService.ListBlobsAsync($"bicep/{configId}/").Returns([blob1, blob2]);
        _blobService.DownloadContentAsync(blob1).Returns("targetScope = 'resourceGroup'");
        _blobService.DownloadContentAsync(blob2).Returns("param name string");

        // Act
        var result = await _sut.GetLatestFilesAsync("bicep", configId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result!["main.bicep"].Should().Be("targetScope = 'resourceGroup'");
        result["modules/storage.bicep"].Should().Be("param name string");
    }

    // ──────────────────────────────────────────────────
    //  UploadArtifactAsync
    // ──────────────────────────────────────────────────

    [Fact]
    public async Task Given_ValidParameters_When_UploadArtifactAsync_Then_ConstructsCorrectBlobPathAsync()
    {
        // Arrange
        var configId = Guid.NewGuid();
        const string timestamp = "20260517-140000";
        const string relativePath = "modules/storage.bicep";
        const string content = "param name string";
        var expectedPath = $"bicep/{configId}/{timestamp}/{relativePath}";
        var expectedUri = new Uri($"https://blob.storage/{expectedPath}");

        _blobService.UploadContentAsync(expectedPath, content, "text/plain").Returns(expectedUri);

        // Act
        var result = await _sut.UploadArtifactAsync("bicep", configId, timestamp, relativePath, content);

        // Assert
        result.Should().Be(expectedUri);
        await _blobService.Received(1).UploadContentAsync(expectedPath, content, "text/plain");
    }

    // ──────────────────────────────────────────────────
    //  Latest-version detection logic
    // ──────────────────────────────────────────────────

    [Fact]
    public async Task Given_ThreeTimestamps_When_GetLatestFilesAsync_Then_OnlyReturnsFilesFromMostRecentTimestampAsync()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var blobs = new List<string>
        {
            $"bicep/{configId}/20260101-080000/main.bicep",
            $"bicep/{configId}/20260315-120000/main.bicep",
            $"bicep/{configId}/20260517-140000/main.bicep",
        };

        _blobService.ListBlobsAsync($"bicep/{configId}/").Returns(blobs);
        _blobService.DownloadContentAsync(blobs[2]).Returns("latest content");

        // Act
        var result = await _sut.GetLatestFilesAsync("bicep", configId);

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKey("main.bicep");
        result!["main.bicep"].Should().Be("latest content");

        await _blobService.DidNotReceive().DownloadContentAsync(blobs[0]);
        await _blobService.DidNotReceive().DownloadContentAsync(blobs[1]);
    }

    [Fact]
    public async Task Given_ListIsCanceled_When_GetLatestFilesAsync_Then_PropagatesCancellationAsync()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var cancellationToken = new CancellationToken(canceled: true);
        var prefix = $"bicep/{configId}/";
        _blobService.ListBlobsAsync(prefix, cancellationToken)
            .Returns(Task.FromCanceled<IReadOnlyList<string>>(cancellationToken));

        // Act
        Func<Task> act = async () => await _sut.GetLatestFilesAsync(
            "bicep", configId, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Given_DownloadIsCanceled_When_GetLatestFilesAsync_Then_PropagatesCancellationAsync()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var cancellationToken = new CancellationToken(canceled: true);
        var prefix = $"bicep/{configId}/";
        var blobName = $"bicep/{configId}/20260517-140000/main.bicep";
        _blobService.ListBlobsAsync(prefix, cancellationToken).Returns([blobName]);
        _blobService.DownloadContentAsync(blobName, cancellationToken)
            .Returns(Task.FromCanceled<string?>(cancellationToken));

        // Act
        Func<Task> act = async () => await _sut.GetLatestFilesAsync(
            "bicep", configId, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
