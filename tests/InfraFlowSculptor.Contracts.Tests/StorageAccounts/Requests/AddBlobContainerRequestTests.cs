using FluentAssertions;
using InfraFlowSculptor.Contracts.StorageAccounts.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.StorageAccounts.Requests;

public sealed class AddBlobContainerRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new AddBlobContainerRequest
        {
            Name = "my-container",
            PublicAccess = "None",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullName_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new AddBlobContainerRequest
        {
            Name = null!,
            PublicAccess = "None",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddBlobContainerRequest.Name)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullPublicAccess_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new AddBlobContainerRequest
        {
            Name = "my-container",
            PublicAccess = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddBlobContainerRequest.PublicAccess)).Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidPublicAccess_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new AddBlobContainerRequest
        {
            Name = "my-container",
            PublicAccess = "InvalidValue",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().NotBeEmpty();
    }
}
