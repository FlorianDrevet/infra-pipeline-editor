using FluentAssertions;
using InfraFlowSculptor.Contracts.AppServicePlans.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.AppServicePlans.Requests;

public sealed class CreateAppServicePlanRequestTests
{
    private const string ValidLocation = "WestEurope";
    private const string ValidOsType = "Linux";

    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new CreateAppServicePlanRequest
        {
            ResourceGroupId = Guid.NewGuid(),
            Name = "plan-prod",
            Location = ValidLocation,
            OsType = ValidOsType,
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
        var sut = new CreateAppServicePlanRequest
        {
            ResourceGroupId = Guid.Empty,
            Name = "plan-prod",
            Location = ValidLocation,
            OsType = ValidOsType,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateAppServicePlanRequest.ResourceGroupId)).Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidOsType_When_Validate_Then_ReturnsEnumError()
    {
        // Arrange
        var sut = new CreateAppServicePlanRequest
        {
            ResourceGroupId = Guid.NewGuid(),
            Name = "plan-prod",
            Location = ValidLocation,
            OsType = "BeOS",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateAppServicePlanRequest.OsType)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullLocation_When_Validate_Then_ReturnsRequiredError()
    {
        // Arrange
        var sut = new CreateAppServicePlanRequest
        {
            ResourceGroupId = Guid.NewGuid(),
            Name = "plan-prod",
            Location = null!,
            OsType = ValidOsType,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateAppServicePlanRequest.Location)).Should().BeTrue();
    }
}
