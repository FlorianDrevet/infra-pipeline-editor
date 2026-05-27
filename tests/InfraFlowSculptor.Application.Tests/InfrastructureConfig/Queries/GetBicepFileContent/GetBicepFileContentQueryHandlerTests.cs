using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetBicepFileContent;
using NSubstitute;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Queries.GetBicepFileContent;

public sealed class GetBicepFileContentQueryHandlerTests
{
    private readonly IBlobService _blobService;
    private readonly GetBicepFileContentQueryHandler _sut;

    public GetBicepFileContentQueryHandlerTests()
    {
        _blobService = Substitute.For<IBlobService>();
        _sut = new GetBicepFileContentQueryHandler(_blobService);
    }

    [Fact]
    public async Task Given_BlobsExistAndFileFound_When_Handle_Then_ReturnsContentAsync()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var query = new GetBicepFileContentQuery(configId, "main.bicep");
        var prefix = $"bicep/{configId}/";

        _blobService.ListBlobsAsync(prefix)
            .Returns(new List<string> { $"{prefix}20260101120000/main.bicep" });

        _blobService.DownloadContentAsync($"{prefix}20260101120000/main.bicep")
            .Returns("resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01'");

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Given_NoBlobsExist_When_Handle_Then_ReturnsNotFoundErrorAsync()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var query = new GetBicepFileContentQuery(configId, "main.bicep");
        var prefix = $"bicep/{configId}/";

        _blobService.ListBlobsAsync(prefix)
            .Returns(new List<string>());

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_BlobFolderExistsButFileNotFound_When_Handle_Then_ReturnsNotFoundErrorAsync()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var query = new GetBicepFileContentQuery(configId, "missing.bicep");
        var prefix = $"bicep/{configId}/";

        _blobService.ListBlobsAsync(prefix)
            .Returns(new List<string> { $"{prefix}20260101120000/main.bicep" });

        _blobService.DownloadContentAsync(Arg.Any<string>())
            .Returns((string?)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }
}
