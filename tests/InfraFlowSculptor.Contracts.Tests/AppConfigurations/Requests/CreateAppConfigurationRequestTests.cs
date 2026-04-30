using FluentAssertions;
using InfraFlowSculptor.Contracts.AppConfigurations.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.AppConfigurations.Requests;

public sealed class CreateAppConfigurationRequestTests
{
    private const string ValidLocation = "WestEurope";

    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new CreateAppConfigurationRequest
        {
            ResourceGroupId = Guid.NewGuid(),
            Name = "appcfg-prod",
            Location = ValidLocation,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyResourceGroupId_When_Validate_Then_ReturnsGuidError()
    {
        // Arrange
        var sut = new CreateAppConfigurationRequest
        {
            ResourceGroupId = Guid.Empty,
            Name = "appcfg-prod",
            Location = ValidLocation,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateAppConfigurationRequest.ResourceGroupId)).Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidLocation_When_Validate_Then_ReturnsEnumError()
    {
        // Arrange
        var sut = new CreateAppConfigurationRequest
        {
            ResourceGroupId = Guid.NewGuid(),
            Name = "appcfg-prod",
            Location = "Mars",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateAppConfigurationRequest.Location)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullName_When_Validate_Then_ReturnsRequiredError()
    {
        // Arrange
        var sut = new CreateAppConfigurationRequest
        {
            ResourceGroupId = Guid.NewGuid(),
            Name = null!,
            Location = ValidLocation,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateAppConfigurationRequest.Name)).Should().BeTrue();
    }
}
