using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectDefaultNamingTemplate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.SetProjectDefaultNamingTemplate;

public sealed class SetProjectDefaultNamingTemplateCommandValidatorTests
{
    private const string ProjectIdProperty = "ProjectId.Value";
    private const string TemplateProperty = nameof(SetProjectDefaultNamingTemplateCommand.Template);

    private readonly SetProjectDefaultNamingTemplateCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = CreateCommand();

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullTemplate_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = CreateCommand(template: null);

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
        var command = CreateCommand(projectId: new ProjectId(Guid.Empty));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == ProjectIdProperty);
    }

    [Fact]
    public void Given_EmptyTemplate_When_Validate_Then_FailsOnTemplate()
    {
        // Arrange
        var command = CreateCommand(template: string.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == TemplateProperty);
    }

    [Fact]
    public void Given_UnknownPlaceholder_When_Validate_Then_FailsOnTemplate()
    {
        // Arrange
        var command = CreateCommand(template: "{name}-{unknown}");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == TemplateProperty);
    }

    [Fact]
    public void Given_TemplateLongerThan500Characters_When_Validate_Then_FailsOnTemplate()
    {
        // Arrange
        var command = CreateCommand(template: new string('a', 501));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == TemplateProperty);
    }

    private static SetProjectDefaultNamingTemplateCommand CreateCommand(
        ProjectId? projectId = null,
        string? template = "{prefix}-{name}-{resourceAbbr}-{env}")
    {
        return new SetProjectDefaultNamingTemplateCommand(
            projectId ?? new ProjectId(Guid.NewGuid()),
            template);
    }
}