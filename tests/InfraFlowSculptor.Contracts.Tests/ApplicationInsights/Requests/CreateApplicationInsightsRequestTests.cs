using FluentAssertions;
using InfraFlowSculptor.Contracts.ApplicationInsights.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.ApplicationInsights.Requests;

public sealed class CreateApplicationInsightsRequestTests
{
    private const string ValidLocation = "WestEurope";

    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new CreateApplicationInsightsRequest
        {
            ResourceGroupId = Guid.NewGuid(),
            Name = "appi-prod",
            Location = ValidLocation,
            LogAnalyticsWorkspaceId = Guid.NewGuid(),
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
        var sut = new CreateApplicationInsightsRequest
        {
            ResourceGroupId = Guid.Empty,
            Name = "appi-prod",
            Location = ValidLocation,
            LogAnalyticsWorkspaceId = Guid.NewGuid(),
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateApplicationInsightsRequest.ResourceGroupId)).Should().BeTrue();
    }

    [Fact]
    public void Given_EmptyLogAnalyticsWorkspaceId_When_Validate_Then_ReturnsGuidError()
    {
        // Arrange
        var sut = new CreateApplicationInsightsRequest
        {
            ResourceGroupId = Guid.NewGuid(),
            Name = "appi-prod",
            Location = ValidLocation,
            LogAnalyticsWorkspaceId = Guid.Empty,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateApplicationInsightsRequest.LogAnalyticsWorkspaceId)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullName_When_Validate_Then_ReturnsRequiredError()
    {
        // Arrange
        var sut = new CreateApplicationInsightsRequest
        {
            ResourceGroupId = Guid.NewGuid(),
            Name = null!,
            Location = ValidLocation,
            LogAnalyticsWorkspaceId = Guid.NewGuid(),
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateApplicationInsightsRequest.Name)).Should().BeTrue();
    }
}
