using FluentAssertions;
using InfraFlowSculptor.Contracts.Projects.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.Projects.Requests;

public sealed class AddProjectEnvironmentRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new AddProjectEnvironmentRequest
        {
            Name = "Production",
            ShortName = "prod",
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
        var sut = new AddProjectEnvironmentRequest
        {
            Name = null!,
            ShortName = "prod",
            Location = "WestEurope",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddProjectEnvironmentRequest.Name)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullShortName_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new AddProjectEnvironmentRequest
        {
            Name = "Production",
            ShortName = null!,
            Location = "WestEurope",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddProjectEnvironmentRequest.ShortName)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullLocation_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new AddProjectEnvironmentRequest
        {
            Name = "Production",
            ShortName = "prod",
            Location = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddProjectEnvironmentRequest.Location)).Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidLocation_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new AddProjectEnvironmentRequest
        {
            Name = "Production",
            ShortName = "prod",
            Location = "InvalidValue",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().NotBeEmpty();
    }
}
