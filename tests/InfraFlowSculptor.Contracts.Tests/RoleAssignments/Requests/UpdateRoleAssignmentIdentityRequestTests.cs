using FluentAssertions;
using InfraFlowSculptor.Contracts.RoleAssignments.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.RoleAssignments.Requests;

public sealed class UpdateRoleAssignmentIdentityRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new UpdateRoleAssignmentIdentityRequest
        {
            ManagedIdentityType = "SystemAssigned",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullManagedIdentityType_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new UpdateRoleAssignmentIdentityRequest
        {
            ManagedIdentityType = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(UpdateRoleAssignmentIdentityRequest.ManagedIdentityType)).Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidManagedIdentityType_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new UpdateRoleAssignmentIdentityRequest
        {
            ManagedIdentityType = "InvalidValue",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().NotBeEmpty();
    }
}
