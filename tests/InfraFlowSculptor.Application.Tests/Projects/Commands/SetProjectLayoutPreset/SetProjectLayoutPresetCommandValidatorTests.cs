using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectLayoutPreset;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.SetProjectLayoutPreset;

public sealed class SetProjectLayoutPresetCommandValidatorTests
{
    private const string ProjectIdProperty = nameof(SetProjectLayoutPresetCommand.ProjectId);
    private const string PresetProperty = nameof(SetProjectLayoutPresetCommand.Preset);

    private readonly SetProjectLayoutPresetCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new SetProjectLayoutPresetCommand(
            ProjectId.CreateUnique(),
            "AllInOne");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullProjectId_When_Validate_Then_FailsOnProjectId()
    {
        // Arrange
        var command = new SetProjectLayoutPresetCommand(null!, "AllInOne");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == ProjectIdProperty);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Given_EmptyPreset_When_Validate_Then_FailsOnPreset(string? preset)
    {
        // Arrange
        var command = new SetProjectLayoutPresetCommand(
            ProjectId.CreateUnique(),
            preset!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == PresetProperty);
    }
}
