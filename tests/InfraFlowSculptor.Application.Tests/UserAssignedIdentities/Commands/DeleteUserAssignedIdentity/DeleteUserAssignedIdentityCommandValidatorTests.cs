using FluentAssertions;
using InfraFlowSculptor.Application.UserAssignedIdentities.Commands.DeleteUserAssignedIdentity;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.UserAssignedIdentities.Commands.DeleteUserAssignedIdentity;

public sealed class DeleteUserAssignedIdentityCommandValidatorTests
{
    private const string IdProperty = nameof(DeleteUserAssignedIdentityCommand.Id);

    private readonly DeleteUserAssignedIdentityCommandValidator _sut = new();

    [Fact]
    public void Given_NonNullId_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new DeleteUserAssignedIdentityCommand(AzureResourceId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_NullId_When_Validate_Then_FailsOnId()
    {
        // Arrange
        var command = new DeleteUserAssignedIdentityCommand(null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == IdProperty);
    }
}
