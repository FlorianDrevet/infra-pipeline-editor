using FluentAssertions;
using InfraFlowSculptor.Contracts.Projects.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.Projects.Requests;

public sealed class AddProjectPipelineVariableGroupRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new AddProjectPipelineVariableGroupRequest
        {
            GroupName = "my-variable-group",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullGroupName_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new AddProjectPipelineVariableGroupRequest
        {
            GroupName = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddProjectPipelineVariableGroupRequest.GroupName)).Should().BeTrue();
    }
}
