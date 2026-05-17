using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.GenerateProjectPipeline;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.GenerateProjectPipeline;

public sealed class GenerateProjectPipelineCommandValidatorTests
{
    private const string ProjectIdProperty = "ProjectId.Value";

    private readonly GenerateProjectPipelineCommandValidator _sut = new();

    [Fact]
    public void Given_ValidProjectId_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new GenerateProjectPipelineCommand(ProjectId.CreateUnique());

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
        var command = new GenerateProjectPipelineCommand(new ProjectId(Guid.Empty));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == ProjectIdProperty);
    }
}
