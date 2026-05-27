using FluentAssertions;
using InfraFlowSculptor.Contracts.ContainerAppEnvironments.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.ContainerAppEnvironments.Requests;

public sealed class CreateContainerAppEnvironmentRequestTests
{
    private const string ValidLocation = "WestEurope";
    private static readonly Guid ValidResourceGroupId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new CreateContainerAppEnvironmentRequest
        {
            Name = "cae-prod",
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
        var sut = new CreateContainerAppEnvironmentRequest
        {
            Name = null!,
            Location = ValidLocation,
            ResourceGroupId = ValidResourceGroupId,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateContainerAppEnvironmentRequest.Name)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullLocation_When_Validate_Then_Error()
    {
        // Arrange
        var sut = new CreateContainerAppEnvironmentRequest
        {
            Name = "cae-prod",
            Location = null!,
            ResourceGroupId = ValidResourceGroupId,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateContainerAppEnvironmentRequest.Location)).Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidLocation_When_Validate_Then_Error()
    {
        // Arrange
        var sut = new CreateContainerAppEnvironmentRequest
        {
            Name = "cae-prod",
            Location = "InvalidRegion",
            ResourceGroupId = ValidResourceGroupId,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateContainerAppEnvironmentRequest.Location)).Should().BeTrue();
    }

    [Fact]
    public void Given_EmptyResourceGroupId_When_Validate_Then_Error()
    {
        // Arrange
        var sut = new CreateContainerAppEnvironmentRequest
        {
            Name = "cae-prod",
            Location = ValidLocation,
            ResourceGroupId = Guid.Empty,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateContainerAppEnvironmentRequest.ResourceGroupId)).Should().BeTrue();
    }

    [Fact]
    public void Given_EmptyLogAnalyticsWorkspaceId_When_Validate_Then_Error()
    {
        // Arrange
        var sut = new CreateContainerAppEnvironmentRequest
        {
            Name = "cae-prod",
            Location = ValidLocation,
            ResourceGroupId = ValidResourceGroupId,
            LogAnalyticsWorkspaceId = Guid.Empty,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateContainerAppEnvironmentRequest.LogAnalyticsWorkspaceId)).Should().BeTrue();
    }
}
