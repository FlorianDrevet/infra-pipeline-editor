using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBicep;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.GenerateProjectBicep;

public sealed class GenerateProjectBicepCommandValidatorTests
{
    private const string ProjectIdProperty = "ProjectId.Value";

    private readonly GenerateProjectBicepCommandValidator _sut = new();

    [Fact]
    public void Given_ValidProjectId_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new GenerateProjectBicepCommand(ProjectId.CreateUnique());

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
        var command = new GenerateProjectBicepCommand(new ProjectId(Guid.Empty));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == ProjectIdProperty);
    }
}
