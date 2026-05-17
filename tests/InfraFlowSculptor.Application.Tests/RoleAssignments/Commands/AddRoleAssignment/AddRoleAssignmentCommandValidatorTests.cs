using FluentAssertions;
using InfraFlowSculptor.Application.RoleAssignments.Commands.AddRoleAssignment;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.RoleAssignments.Commands.AddRoleAssignment;

public sealed class AddRoleAssignmentCommandValidatorTests
{
    private readonly AddRoleAssignmentCommandValidator _sut = new();

    [Fact]
    public void Given_ValidSystemAssigned_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new AddRoleAssignmentCommand(
            AzureResourceId.CreateUnique(),
            AzureResourceId.CreateUnique(),
            nameof(ManagedIdentityType.IdentityTypeEnum.SystemAssigned),
            AzureRoleDefinitionCatalog.KeyVaultSecretsUser,
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
        var command = new AddRoleAssignmentCommand(
            AzureResourceId.CreateUnique(),
            AzureResourceId.CreateUnique(),
            nameof(ManagedIdentityType.IdentityTypeEnum.UserAssigned),
            AzureRoleDefinitionCatalog.KeyVaultSecretsUser,
            UserAssignedIdentityId: AzureResourceId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyRoleDefinitionId_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new AddRoleAssignmentCommand(
            AzureResourceId.CreateUnique(),
            AzureResourceId.CreateUnique(),
            nameof(ManagedIdentityType.IdentityTypeEnum.SystemAssigned),
            RoleDefinitionId: "",
            UserAssignedIdentityId: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddRoleAssignmentCommand.RoleDefinitionId));
    }

    [Fact]
    public void Given_InvalidManagedIdentityType_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new AddRoleAssignmentCommand(
            AzureResourceId.CreateUnique(),
            AzureResourceId.CreateUnique(),
            ManagedIdentityType: "BadType",
            AzureRoleDefinitionCatalog.KeyVaultSecretsUser,
            UserAssignedIdentityId: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddRoleAssignmentCommand.ManagedIdentityType));
    }

    [Fact]
    public void Given_SameSourceAndTarget_When_Validate_Then_Fails()
    {
        // Arrange
        var resourceId = AzureResourceId.CreateUnique();
        var command = new AddRoleAssignmentCommand(
            resourceId,
            resourceId,
            nameof(ManagedIdentityType.IdentityTypeEnum.SystemAssigned),
            AzureRoleDefinitionCatalog.KeyVaultSecretsUser,
            UserAssignedIdentityId: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddRoleAssignmentCommand.TargetResourceId));
    }

    [Fact]
    public void Given_UserAssignedWithoutUaiId_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new AddRoleAssignmentCommand(
            AzureResourceId.CreateUnique(),
            AzureResourceId.CreateUnique(),
            nameof(ManagedIdentityType.IdentityTypeEnum.UserAssigned),
            AzureRoleDefinitionCatalog.KeyVaultSecretsUser,
            UserAssignedIdentityId: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddRoleAssignmentCommand.UserAssignedIdentityId));
    }

    [Fact]
    public void Given_AcrPullWithSystemAssigned_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new AddRoleAssignmentCommand(
            AzureResourceId.CreateUnique(),
            AzureResourceId.CreateUnique(),
            nameof(ManagedIdentityType.IdentityTypeEnum.SystemAssigned),
            AzureRoleDefinitionCatalog.AcrPull,
            UserAssignedIdentityId: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
