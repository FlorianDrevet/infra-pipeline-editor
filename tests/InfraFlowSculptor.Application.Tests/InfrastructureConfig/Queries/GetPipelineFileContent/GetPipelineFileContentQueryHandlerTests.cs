using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetPipelineFileContent;
using NSubstitute;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Queries.GetPipelineFileContent;

public sealed class GetPipelineFileContentQueryHandlerTests
{
    private readonly IGeneratedArtifactService _artifactService;
    private readonly GetPipelineFileContentQueryHandler _sut;

    public GetPipelineFileContentQueryHandlerTests()
    {
        _artifactService = Substitute.For<IGeneratedArtifactService>();
        _sut = new GetPipelineFileContentQueryHandler(_artifactService);
    }

    [Fact]
    public async Task Given_FileExists_When_Handle_Then_ReturnsContentAsync()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var filePath = "azure-pipelines.yml";
        var query = new GetPipelineFileContentQuery(configId, filePath);
        var expectedContent = "trigger:\n  - main";

        _artifactService.GetFileContentAsync("pipeline", configId, filePath, Arg.Any<CancellationToken>())
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
        var filePath = "missing-pipeline.yml";
        var query = new GetPipelineFileContentQuery(configId, filePath);

        _artifactService.GetFileContentAsync("pipeline", configId, filePath, Arg.Any<CancellationToken>())
            .Returns((string?)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }
}
