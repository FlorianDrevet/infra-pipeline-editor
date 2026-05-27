using FluentAssertions;
using InfraFlowSculptor.Contracts.Projects.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.Projects.Requests;

public sealed class UpdateProjectEnvironmentRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new UpdateProjectEnvironmentRequest
        {
            Name = "Production",
            ShortName = "prod",
            Location = "WestEurope",
            SubscriptionId = Guid.NewGuid(),
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
        var sut = new UpdateProjectEnvironmentRequest
        {
            Name = null!,
            ShortName = "prod",
            Location = "WestEurope",
            SubscriptionId = Guid.NewGuid(),
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(UpdateProjectEnvironmentRequest.Name)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullShortName_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new UpdateProjectEnvironmentRequest
        {
            Name = "Production",
            ShortName = null!,
            Location = "WestEurope",
            SubscriptionId = Guid.NewGuid(),
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(UpdateProjectEnvironmentRequest.ShortName)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullLocation_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new UpdateProjectEnvironmentRequest
        {
            Name = "Production",
            ShortName = "prod",
            Location = null!,
            SubscriptionId = Guid.NewGuid(),
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(UpdateProjectEnvironmentRequest.Location)).Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidLocation_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new UpdateProjectEnvironmentRequest
        {
            Name = "Production",
            ShortName = "prod",
            Location = "InvalidValue",
            SubscriptionId = Guid.NewGuid(),
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().NotBeEmpty();
    }

    [Fact]
    public void Given_EmptySubscriptionId_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new UpdateProjectEnvironmentRequest
        {
            Name = "Production",
            ShortName = "prod",
            Location = "WestEurope",
            SubscriptionId = Guid.Empty,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(UpdateProjectEnvironmentRequest.SubscriptionId)).Should().BeTrue();
    }
}
