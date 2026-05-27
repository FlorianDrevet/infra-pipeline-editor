using FluentAssertions;
using InfraFlowSculptor.Contracts.Tests.TestSupport;
using InfraFlowSculptor.Contracts.UserAssignedIdentities.Requests;

namespace InfraFlowSculptor.Contracts.Tests.UserAssignedIdentities.Requests;

public sealed class UpdateUserAssignedIdentityRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new UpdateUserAssignedIdentityRequest
        {
            Name = "my-identity",
            Location = "WestEurope",
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
        var sut = new UpdateUserAssignedIdentityRequest
        {
            Name = null!,
            Location = "WestEurope",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(UpdateUserAssignedIdentityRequest.Name)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullLocation_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new UpdateUserAssignedIdentityRequest
        {
            Name = "my-identity",
            Location = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(UpdateUserAssignedIdentityRequest.Location)).Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidLocation_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new UpdateUserAssignedIdentityRequest
        {
            Name = "my-identity",
            Location = "InvalidValue",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().NotBeEmpty();
    }
}
