using FluentAssertions;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.InfrastructureConfig.Requests;

public sealed class PushBicepToGitRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new PushBicepToGitRequest
        {
            BranchName = "feature/bicep-output",
            CommitMessage = "Add generated Bicep files",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullBranchName_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new PushBicepToGitRequest
        {
            BranchName = null!,
            CommitMessage = "Add generated Bicep files",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(PushBicepToGitRequest.BranchName)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullCommitMessage_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new PushBicepToGitRequest
        {
            BranchName = "feature/bicep-output",
            CommitMessage = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(PushBicepToGitRequest.CommitMessage)).Should().BeTrue();
    }
}
