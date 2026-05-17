using FluentAssertions;
using InfraFlowSculptor.Application.RoleAssignments.Commands.UpdateRoleAssignmentIdentity;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.RoleAssignments.Commands.UpdateRoleAssignmentIdentity;

public sealed class UpdateRoleAssignmentIdentityCommandValidatorTests
{
    private readonly UpdateRoleAssignmentIdentityCommandValidator _sut = new();

    [Fact]
    public void Given_ValidSystemAssigned_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new UpdateRoleAssignmentIdentityCommand(
            AzureResourceId.CreateUnique(),
            RoleAssignmentId.CreateUnique(),
            nameof(ManagedIdentityType.IdentityTypeEnum.SystemAssigned),
            UserAssignedIdentityId: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_ValidUserAssigned_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new UpdateRoleAssignmentIdentityCommand(
            AzureResourceId.CreateUnique(),
            RoleAssignmentId.CreateUnique(),
            nameof(ManagedIdentityType.IdentityTypeEnum.UserAssigned),
            UserAssignedIdentityId: AzureResourceId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_InvalidManagedIdentityType_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new UpdateRoleAssignmentIdentityCommand(
            AzureResourceId.CreateUnique(),
            RoleAssignmentId.CreateUnique(),
            ManagedIdentityType: "Invalid",
            UserAssignedIdentityId: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateRoleAssignmentIdentityCommand.ManagedIdentityType));
    }

    [Fact]
    public void Given_UserAssignedWithoutUaiId_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new UpdateRoleAssignmentIdentityCommand(
            AzureResourceId.CreateUnique(),
            RoleAssignmentId.CreateUnique(),
            nameof(ManagedIdentityType.IdentityTypeEnum.UserAssigned),
            UserAssignedIdentityId: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateRoleAssignmentIdentityCommand.UserAssignedIdentityId));
    }
}
