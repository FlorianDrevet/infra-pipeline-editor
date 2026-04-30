using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.CreateProject;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.CreateProject;

public sealed class CreateProjectCommandValidatorTests
{
    private const string ValidName = "RetailApi";
    private const string ValidDescription = "Retail APIs";
    private const string NameProperty = nameof(CreateProjectCommand.Name);
    private const string DescriptionProperty = nameof(CreateProjectCommand.Description);

    private readonly CreateProjectCommandValidator _sut = new();

    [Fact]
    public void Given_ValidNameAndDescription_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new CreateProjectCommand(ValidName, ValidDescription);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullDescription_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new CreateProjectCommand(ValidName, null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Given_MissingName_When_Validate_Then_FailsOnName(string? name)
    {
        // Arrange
        var command = new CreateProjectCommand(name!, ValidDescription);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == NameProperty);
    }

    [Fact]
    public void Given_NameLongerThan100Characters_When_Validate_Then_FailsOnName()
    {
        // Arrange
        var name = new string('A', 101);
        var command = new CreateProjectCommand(name, ValidDescription);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == NameProperty);
    }

    [Fact]
    public void Given_DescriptionLongerThan500Characters_When_Validate_Then_FailsOnDescription()
    {
        // Arrange
        var description = new string('A', 501);
        var command = new CreateProjectCommand(ValidName, description);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == DescriptionProperty);
    }
}
