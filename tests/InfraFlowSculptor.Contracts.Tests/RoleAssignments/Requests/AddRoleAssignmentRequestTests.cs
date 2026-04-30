using FluentAssertions;
using InfraFlowSculptor.Contracts.RoleAssignments.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.RoleAssignments.Requests;

public sealed class AddRoleAssignmentRequestTests
{
    private const string SystemAssigned = "SystemAssigned";
    private const string ValidRoleDefinitionId = "/providers/Microsoft.Authorization/roleDefinitions/00000000-0000-0000-0000-000000000001";

    [Fact]
    public void Given_ValidSystemAssignedRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new AddRoleAssignmentRequest
        {
            TargetResourceId = Guid.NewGuid(),
            ManagedIdentityType = SystemAssigned,
            RoleDefinitionId = ValidRoleDefinitionId,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyTargetResourceId_When_Validate_Then_ReturnsGuidError()
    {
        // Arrange
        var sut = new AddRoleAssignmentRequest
        {
            TargetResourceId = Guid.Empty,
            ManagedIdentityType = SystemAssigned,
            RoleDefinitionId = ValidRoleDefinitionId,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddRoleAssignmentRequest.TargetResourceId)).Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidManagedIdentityType_When_Validate_Then_ReturnsEnumError()
    {
        // Arrange
        var sut = new AddRoleAssignmentRequest
        {
            TargetResourceId = Guid.NewGuid(),
            ManagedIdentityType = "GroupAssigned",
            RoleDefinitionId = ValidRoleDefinitionId,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().HaveCount(1);
    }

    [Fact]
    public void Given_NullRoleDefinitionId_When_Validate_Then_ReturnsRequiredError()
    {
        // Arrange
        var sut = new AddRoleAssignmentRequest
        {
            TargetResourceId = Guid.NewGuid(),
            ManagedIdentityType = SystemAssigned,
            RoleDefinitionId = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddRoleAssignmentRequest.RoleDefinitionId)).Should().BeTrue();
    }
}
