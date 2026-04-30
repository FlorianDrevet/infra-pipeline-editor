using FluentAssertions;
using InfraFlowSculptor.Contracts.ResourceGroups.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.ResourceGroups.Requests;

public sealed class CreateResourceGroupRequestTests
{
    private const string ValidLocation = "WestEurope";

    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new CreateResourceGroupRequest
        {
            InfraConfigId = Guid.NewGuid(),
            Name = "rg-prod",
            Location = ValidLocation,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyInfraConfigId_When_Validate_Then_ReturnsGuidError()
    {
        // Arrange
        var sut = new CreateResourceGroupRequest
        {
            InfraConfigId = Guid.Empty,
            Name = "rg-prod",
            Location = ValidLocation,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateResourceGroupRequest.InfraConfigId)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullName_When_Validate_Then_ReturnsRequiredError()
    {
        // Arrange
        var sut = new CreateResourceGroupRequest
        {
            InfraConfigId = Guid.NewGuid(),
            Name = null!,
            Location = ValidLocation,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateResourceGroupRequest.Name)).Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidLocation_When_Validate_Then_ReturnsEnumError()
    {
        // Arrange
        var sut = new CreateResourceGroupRequest
        {
            InfraConfigId = Guid.NewGuid(),
            Name = "rg-prod",
            Location = "Pluto",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateResourceGroupRequest.Location)).Should().BeTrue();
    }
}
