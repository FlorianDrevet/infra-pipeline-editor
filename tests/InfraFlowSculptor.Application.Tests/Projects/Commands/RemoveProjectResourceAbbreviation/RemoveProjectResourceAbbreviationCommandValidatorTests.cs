using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectResourceAbbreviation;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.RemoveProjectResourceAbbreviation;

public sealed class RemoveProjectResourceAbbreviationCommandValidatorTests
{
    private const string ProjectIdProperty = "ProjectId.Value";
    private const string ResourceTypeProperty = nameof(RemoveProjectResourceAbbreviationCommand.ResourceType);

    private readonly RemoveProjectResourceAbbreviationCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new RemoveProjectResourceAbbreviationCommand(
            ProjectId.CreateUnique(),
            "Microsoft.Storage/storageAccounts");

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
        var command = new RemoveProjectResourceAbbreviationCommand(
            new ProjectId(Guid.Empty),
            "Microsoft.Storage/storageAccounts");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == ProjectIdProperty);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Given_EmptyResourceType_When_Validate_Then_FailsOnResourceType(string? resourceType)
    {
        // Arrange
        var command = new RemoveProjectResourceAbbreviationCommand(
            ProjectId.CreateUnique(),
            resourceType!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == ResourceTypeProperty);
    }

    [Fact]
    public void Given_ResourceTypeTooLong_When_Validate_Then_FailsOnResourceType()
    {
        // Arrange
        var command = new RemoveProjectResourceAbbreviationCommand(
            ProjectId.CreateUnique(),
            new string('a', 101));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == ResourceTypeProperty);
    }
}
