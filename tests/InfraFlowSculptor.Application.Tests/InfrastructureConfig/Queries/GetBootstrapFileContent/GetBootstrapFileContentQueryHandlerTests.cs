using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetBootstrapFileContent;
using NSubstitute;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Queries.GetBootstrapFileContent;

public sealed class GetBootstrapFileContentQueryHandlerTests
{
    private readonly IGeneratedArtifactService _artifactService;
    private readonly GetBootstrapFileContentQueryHandler _sut;

    public GetBootstrapFileContentQueryHandlerTests()
    {
        _artifactService = Substitute.For<IGeneratedArtifactService>();
        _sut = new GetBootstrapFileContentQueryHandler(_artifactService);
    }

    [Fact]
    public async Task Given_FileExists_When_Handle_Then_ReturnsContentAsync()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var filePath = "bootstrap.pipeline.yml";
        var query = new GetBootstrapFileContentQuery(configId, filePath);
        var expectedContent = "trigger: none";

        _artifactService.GetFileContentAsync("bootstrap", configId, filePath, Arg.Any<CancellationToken>())
            .Returns(expectedContent);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Content.Should().Be(expectedContent);
    }

    [Fact]
    public async Task Given_FileNotFound_When_Handle_Then_ReturnsNotFoundErrorAsync()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var filePath = "missing-bootstrap.yml";
        var query = new GetBootstrapFileContentQuery(configId, filePath);

        _artifactService.GetFileContentAsync("bootstrap", configId, filePath, Arg.Any<CancellationToken>())
            .Returns((string?)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }
}
