using FluentAssertions;
using InfraFlowSculptor.Contracts.FrontDoors.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.FrontDoors.Requests;

public sealed class CreateFrontDoorRequestTests
{
    private const string ValidLocation = "WestEurope";
    private static readonly Guid ValidResourceGroupId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new CreateFrontDoorRequest
        {
            Name = "fd-prod",
            Location = ValidLocation,
            ResourceGroupId = ValidResourceGroupId,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullName_When_Validate_Then_Error()
    {
        // Arrange
        var sut = new CreateFrontDoorRequest
        {
            Name = null!,
            Location = ValidLocation,
            ResourceGroupId = ValidResourceGroupId,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateFrontDoorRequest.Name)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullLocation_When_Validate_Then_Error()
    {
        // Arrange
        var sut = new CreateFrontDoorRequest
        {
            Name = "fd-prod",
            Location = null!,
            ResourceGroupId = ValidResourceGroupId,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateFrontDoorRequest.Location)).Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidLocation_When_Validate_Then_Error()
    {
        // Arrange
        var sut = new CreateFrontDoorRequest
        {
            Name = "fd-prod",
            Location = "InvalidRegion",
            ResourceGroupId = ValidResourceGroupId,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateFrontDoorRequest.Location)).Should().BeTrue();
    }

    [Fact]
    public void Given_EmptyResourceGroupId_When_Validate_Then_Error()
    {
        // Arrange
        var sut = new CreateFrontDoorRequest
        {
            Name = "fd-prod",
            Location = ValidLocation,
            ResourceGroupId = Guid.Empty,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateFrontDoorRequest.ResourceGroupId)).Should().BeTrue();
    }
}
