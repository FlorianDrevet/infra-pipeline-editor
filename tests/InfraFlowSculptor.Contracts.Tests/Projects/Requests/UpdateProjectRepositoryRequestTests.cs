using FluentAssertions;
using InfraFlowSculptor.Contracts.Projects.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.Projects.Requests;

public sealed class UpdateProjectRepositoryRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new UpdateProjectRepositoryRequest
        {
            RepositoryUrl = "https://github.com/org/repo",
            DefaultBranch = "main",
            PersonalAccessToken = "token",
            ContentKinds = ["Infrastructure"],
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_InvalidUrl_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new UpdateProjectRepositoryRequest
        {
            RepositoryUrl = "not-a-url",
            ContentKinds = ["Infrastructure"],
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(UpdateProjectRepositoryRequest.RepositoryUrl)).Should().BeTrue();
    }

    [Fact]
    public void Given_DefaultBranchOver200Chars_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new UpdateProjectRepositoryRequest
        {
            DefaultBranch = new string('x', 201),
            ContentKinds = ["Infrastructure"],
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(UpdateProjectRepositoryRequest.DefaultBranch)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullContentKinds_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new UpdateProjectRepositoryRequest
        {
            ContentKinds = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(UpdateProjectRepositoryRequest.ContentKinds)).Should().BeTrue();
    }

    [Fact]
    public void Given_EmptyContentKinds_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new UpdateProjectRepositoryRequest
        {
            ContentKinds = [],
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(UpdateProjectRepositoryRequest.ContentKinds)).Should().BeTrue();
    }
}
