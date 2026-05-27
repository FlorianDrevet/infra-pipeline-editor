using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectTags;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.SetProjectTags;

public sealed class SetProjectTagsCommandValidatorTests
{
    private const string ProjectIdProperty = nameof(SetProjectTagsCommand.ProjectId);

    private readonly SetProjectTagsCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new SetProjectTagsCommand(
            Guid.NewGuid(),
            [("Environment", "Production")]);

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
        var command = new SetProjectTagsCommand(
            Guid.Empty,
            [("Environment", "Production")]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == ProjectIdProperty);
    }

    [Fact]
    public void Given_TooManyTags_When_Validate_Then_Fails()
    {
        // Arrange
        var tags = Enumerable.Range(0, 16)
            .Select(i => ($"Key{i}", $"Value{i}"))
            .ToList();
        var command = new SetProjectTagsCommand(Guid.NewGuid(), tags);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetProjectTagsCommand.Tags));
    }

    [Fact]
    public void Given_TagWithEmptyName_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new SetProjectTagsCommand(
            Guid.NewGuid(),
            [("", "Value")]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Item1"));
    }

    [Fact]
    public void Given_TagWithEmptyValue_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new SetProjectTagsCommand(
            Guid.NewGuid(),
            [("Key", "")]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Item2"));
    }

    [Fact]
    public void Given_TagNameTooLong_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new SetProjectTagsCommand(
            Guid.NewGuid(),
            [(new string('a', 513), "Value")]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Item1"));
    }

    [Fact]
    public void Given_TagValueTooLong_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new SetProjectTagsCommand(
            Guid.NewGuid(),
            [("Key", new string('a', 257))]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Item2"));
    }
}
