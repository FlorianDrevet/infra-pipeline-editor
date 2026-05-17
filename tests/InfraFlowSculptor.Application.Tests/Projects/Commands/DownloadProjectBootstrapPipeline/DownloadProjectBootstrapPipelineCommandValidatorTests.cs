using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.DownloadProjectBootstrapPipeline;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.DownloadProjectBootstrapPipeline;

public sealed class DownloadProjectBootstrapPipelineCommandValidatorTests
{
    private const string ProjectIdProperty = "ProjectId.Value";

    private readonly DownloadProjectBootstrapPipelineCommandValidator _sut = new();

    [Fact]
    public void Given_ValidProjectId_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new DownloadProjectBootstrapPipelineCommand(ProjectId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyProjectId_When_Validate_Then_FailsOnProjectId()
    {
        // Arrange
        var command = new DownloadProjectBootstrapPipelineCommand(new ProjectId(Guid.Empty));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == ProjectIdProperty);
    }
}
