using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectResourceAbbreviation;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.SetProjectResourceAbbreviation;

public sealed class SetProjectResourceAbbreviationCommandValidatorTests
{
    private const string ResourceTypeProperty = nameof(SetProjectResourceAbbreviationCommand.ResourceType);

    private readonly SetProjectResourceAbbreviationCommandValidator _sut = new();

    [Fact]
    public void Given_SupportedResourceType_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = CreateCommand(resourceType: AzureResourceTypes.StorageAccount);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_UnknownResourceType_When_Validate_Then_FailsOnResourceType()
    {
        // Arrange
        var command = CreateCommand(resourceType: "UnknownResourceType");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == ResourceTypeProperty);
    }

    private static SetProjectResourceAbbreviationCommand CreateCommand(
        string resourceType,
        string abbreviation = "stg")
    {
        return new SetProjectResourceAbbreviationCommand(
            new ProjectId(Guid.NewGuid()),
            resourceType,
            abbreviation);
    }
}