using FluentAssertions;
using InfraFlowSculptor.Contracts.Projects.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.Projects.Requests;

public sealed class AddProjectMemberRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new AddProjectMemberRequest
        {
            UserId = Guid.NewGuid(),
            Role = "Contributor",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_DefaultUserId_When_Validate_Then_NoError()
    {
        // Arrange — [Required] on value type Guid does not reject Guid.Empty
        var sut = new AddProjectMemberRequest
        {
            UserId = Guid.Empty,
            Role = "Contributor",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullRole_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new AddProjectMemberRequest
        {
            UserId = Guid.NewGuid(),
            Role = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddProjectMemberRequest.Role)).Should().BeTrue();
    }
}
