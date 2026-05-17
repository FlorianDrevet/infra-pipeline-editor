using FluentAssertions;
using InfraFlowSculptor.Contracts.StorageAccounts.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.StorageAccounts.Requests;

public sealed class UpdateBlobContainerPublicAccessRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new UpdateBlobContainerPublicAccessRequest
        {
            PublicAccess = "None",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullPublicAccess_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new UpdateBlobContainerPublicAccessRequest
        {
            PublicAccess = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(UpdateBlobContainerPublicAccessRequest.PublicAccess)).Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidPublicAccess_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new UpdateBlobContainerPublicAccessRequest
        {
            PublicAccess = "InvalidValue",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().NotBeEmpty();
    }
}
