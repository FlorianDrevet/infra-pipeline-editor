using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectResourceNamingTemplate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.RemoveProjectResourceNamingTemplate;

public sealed class RemoveProjectResourceNamingTemplateCommandValidatorTests
{
    private const string ProjectIdProperty = "ProjectId.Value";
    private const string ResourceTypeProperty = nameof(RemoveProjectResourceNamingTemplateCommand.ResourceType);

    private readonly RemoveProjectResourceNamingTemplateCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new RemoveProjectResourceNamingTemplateCommand(
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
        var command = new RemoveProjectResourceNamingTemplateCommand(
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
        var command = new RemoveProjectResourceNamingTemplateCommand(
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
        var command = new RemoveProjectResourceNamingTemplateCommand(
            ProjectId.CreateUnique(),
            new string('a', 101));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == ResourceTypeProperty);
    }
}
