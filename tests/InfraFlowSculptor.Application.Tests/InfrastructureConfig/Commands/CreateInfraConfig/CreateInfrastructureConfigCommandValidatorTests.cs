using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.CreateInfraConfig;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.CreateInfraConfig;

public sealed class CreateInfrastructureConfigCommandValidatorTests
{
    private const string NameProperty = nameof(CreateInfrastructureConfigCommand.Name);

    private readonly CreateInfrastructureConfigCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new CreateInfrastructureConfigCommand("infra-prod", Guid.NewGuid());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Given_MissingName_When_Validate_Then_FailsOnName(string? name)
    {
        // Arrange
        var command = new CreateInfrastructureConfigCommand(name!, Guid.NewGuid());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == NameProperty);
    }

    [Fact]
    public void Given_NameLongerThan100Characters_When_Validate_Then_FailsOnName()
    {
        // Arrange
        var command = new CreateInfrastructureConfigCommand(new string('a', 101), Guid.NewGuid());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == NameProperty);
    }
}