using FluentAssertions;
using InfraFlowSculptor.Application.PersonalAccessTokens.Commands.CreatePersonalAccessToken;

namespace InfraFlowSculptor.Application.Tests.PersonalAccessTokens.Commands.CreatePersonalAccessToken;

public sealed class CreatePersonalAccessTokenCommandValidatorTests
{
    private readonly CreatePersonalAccessTokenCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new CreatePersonalAccessTokenCommand("my-token", DateTime.UtcNow.AddDays(30));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_ValidCommandWithNoExpiration_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new CreatePersonalAccessTokenCommand("my-token", null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_FailsOnName()
    {
        // Arrange
        var command = new CreatePersonalAccessTokenCommand("", DateTime.UtcNow.AddDays(30));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePersonalAccessTokenCommand.Name));
    }

    [Fact]
    public void Given_NameTooLong_When_Validate_Then_FailsOnName()
    {
        // Arrange
        var command = new CreatePersonalAccessTokenCommand(new string('a', 101), DateTime.UtcNow.AddDays(30));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePersonalAccessTokenCommand.Name));
    }

    [Fact]
    public void Given_ExpiredDate_When_Validate_Then_FailsOnExpiresAt()
    {
        // Arrange
        var command = new CreatePersonalAccessTokenCommand("my-token", DateTime.UtcNow.AddDays(-1));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePersonalAccessTokenCommand.ExpiresAt));
    }
}
