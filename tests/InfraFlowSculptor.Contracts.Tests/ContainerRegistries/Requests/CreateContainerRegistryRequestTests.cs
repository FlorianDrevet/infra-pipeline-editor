using FluentAssertions;
using InfraFlowSculptor.Contracts.ContainerRegistries.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.ContainerRegistries.Requests;

public sealed class CreateContainerRegistryRequestTests
{
    private const string ValidLocation = "WestEurope";
    private static readonly Guid ValidResourceGroupId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new CreateContainerRegistryRequest
        {
            Name = "crprod",
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
        var sut = new CreateContainerRegistryRequest
        {
            Name = null!,
            Location = ValidLocation,
            ResourceGroupId = ValidResourceGroupId,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateContainerRegistryRequest.Name)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullLocation_When_Validate_Then_Error()
    {
        // Arrange
        var sut = new CreateContainerRegistryRequest
        {
            Name = "crprod",
            Location = null!,
            ResourceGroupId = ValidResourceGroupId,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateContainerRegistryRequest.Location)).Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidLocation_When_Validate_Then_Error()
    {
        // Arrange
        var sut = new CreateContainerRegistryRequest
        {
            Name = "crprod",
            Location = "InvalidRegion",
            ResourceGroupId = ValidResourceGroupId,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateContainerRegistryRequest.Location)).Should().BeTrue();
    }

    [Fact]
    public void Given_EmptyResourceGroupId_When_Validate_Then_Error()
    {
        // Arrange
        var sut = new CreateContainerRegistryRequest
        {
            Name = "crprod",
            Location = ValidLocation,
            ResourceGroupId = Guid.Empty,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateContainerRegistryRequest.ResourceGroupId)).Should().BeTrue();
    }
}
