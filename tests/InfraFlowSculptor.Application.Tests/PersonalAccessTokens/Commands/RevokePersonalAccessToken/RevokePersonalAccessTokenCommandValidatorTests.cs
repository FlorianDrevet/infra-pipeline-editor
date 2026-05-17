using FluentAssertions;
using InfraFlowSculptor.Application.PersonalAccessTokens.Commands.RevokePersonalAccessToken;
using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.PersonalAccessTokens.Commands.RevokePersonalAccessToken;

public sealed class RevokePersonalAccessTokenCommandValidatorTests
{
    private const string IdProperty = nameof(RevokePersonalAccessTokenCommand.Id);

    private readonly RevokePersonalAccessTokenCommandValidator _sut = new();

    [Fact]
    public void Given_ValidId_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new RevokePersonalAccessTokenCommand(PersonalAccessTokenId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullId_When_Validate_Then_FailsOnId()
    {
        // Arrange
        var command = new RevokePersonalAccessTokenCommand(null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == IdProperty);
    }
}
