using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBootstrapPipeline;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.GenerateProjectBootstrapPipeline;

public sealed class GenerateProjectBootstrapPipelineCommandValidatorTests
{
    private const string ProjectIdProperty = "ProjectId.Value";

    private readonly GenerateProjectBootstrapPipelineCommandValidator _sut = new();

    [Fact]
    public void Given_ValidProjectId_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new GenerateProjectBootstrapPipelineCommand(ProjectId.CreateUnique());

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
        var command = new GenerateProjectBootstrapPipelineCommand(new ProjectId(Guid.Empty));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == ProjectIdProperty);
    }
}
