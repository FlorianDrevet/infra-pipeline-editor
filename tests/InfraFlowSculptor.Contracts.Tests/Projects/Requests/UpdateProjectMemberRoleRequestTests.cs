using FluentAssertions;
using InfraFlowSculptor.Contracts.Projects.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.Projects.Requests;

public sealed class UpdateProjectMemberRoleRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new UpdateProjectMemberRoleRequest
        {
            NewRole = "Owner",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullNewRole_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new UpdateProjectMemberRoleRequest
        {
            NewRole = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(UpdateProjectMemberRoleRequest.NewRole)).Should().BeTrue();
    }
}
