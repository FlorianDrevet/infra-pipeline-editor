using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectResourceNamingTemplate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.SetProjectResourceNamingTemplate;

public sealed class SetProjectResourceNamingTemplateCommandValidatorTests
{
    private const string ResourceTypeProperty = nameof(SetProjectResourceNamingTemplateCommand.ResourceType);

    private readonly SetProjectResourceNamingTemplateCommandValidator _sut = new();

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

    private static SetProjectResourceNamingTemplateCommand CreateCommand(
        string resourceType,
        string template = "{name}")
    {
        return new SetProjectResourceNamingTemplateCommand(
            new ProjectId(Guid.NewGuid()),
            resourceType,
            template);
    }
}
