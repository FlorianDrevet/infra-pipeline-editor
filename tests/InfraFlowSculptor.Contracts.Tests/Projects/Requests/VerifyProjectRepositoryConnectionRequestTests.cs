using FluentAssertions;
using InfraFlowSculptor.Contracts.Projects.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.Projects.Requests;

public sealed class VerifyProjectRepositoryConnectionRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new VerifyProjectRepositoryConnectionRequest
        {
            ProviderType = "GitHub",
            RepositoryUrl = "https://github.com/org/repo",
            PersonalAccessToken = "token",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_InvalidRepositoryUrl_When_Validate_Then_ReturnsUrlError()
    {
        // Arrange
        var sut = new VerifyProjectRepositoryConnectionRequest
        {
            ProviderType = "GitHub",
            RepositoryUrl = "not a url",
            PersonalAccessToken = "token",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(VerifyProjectRepositoryConnectionRequest.RepositoryUrl)).Should().BeTrue();
    }
}