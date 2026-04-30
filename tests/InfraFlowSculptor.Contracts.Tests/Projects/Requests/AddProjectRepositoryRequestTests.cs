using FluentAssertions;
using InfraFlowSculptor.Contracts.Projects.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.Projects.Requests;

public sealed class AddProjectRepositoryRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new AddProjectRepositoryRequest
        {
            Alias = "infra",
            ProviderType = "GitHub",
            RepositoryUrl = "https://github.com/org/repo",
            DefaultBranch = "main",
            ContentKinds = ["Infrastructure"],
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullAlias_When_Validate_Then_ReturnsRequiredError()
    {
        // Arrange
        var sut = new AddProjectRepositoryRequest
        {
            Alias = null!,
            ContentKinds = ["Infrastructure"],
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddProjectRepositoryRequest.Alias)).Should().BeTrue();
    }

    [Fact]
    public void Given_AliasExceedingMaxLength_When_Validate_Then_ReturnsLengthError()
    {
        // Arrange
        var sut = new AddProjectRepositoryRequest
        {
            Alias = new string('a', 51),
            ContentKinds = ["Infrastructure"],
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddProjectRepositoryRequest.Alias)).Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidRepositoryUrl_When_Validate_Then_ReturnsUrlError()
    {
        // Arrange
        var sut = new AddProjectRepositoryRequest
        {
            Alias = "infra",
            RepositoryUrl = "not a url",
            ContentKinds = ["Infrastructure"],
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddProjectRepositoryRequest.RepositoryUrl)).Should().BeTrue();
    }

    [Fact]
    public void Given_DefaultBranchExceedingMaxLength_When_Validate_Then_ReturnsLengthError()
    {
        // Arrange
        var sut = new AddProjectRepositoryRequest
        {
            Alias = "infra",
            DefaultBranch = new string('b', 201),
            ContentKinds = ["Infrastructure"],
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddProjectRepositoryRequest.DefaultBranch)).Should().BeTrue();
    }

    [Fact]
    public void Given_EmptyContentKinds_When_Validate_Then_ReturnsMinLengthError()
    {
        // Arrange
        var sut = new AddProjectRepositoryRequest
        {
            Alias = "infra",
            ContentKinds = [],
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddProjectRepositoryRequest.ContentKinds)).Should().BeTrue();
    }
}
