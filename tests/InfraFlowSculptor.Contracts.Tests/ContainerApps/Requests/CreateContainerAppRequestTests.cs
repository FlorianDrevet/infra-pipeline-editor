using FluentAssertions;
using InfraFlowSculptor.Contracts.ContainerApps.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.ContainerApps.Requests;

public sealed class CreateContainerAppRequestTests
{
    private const string ValidLocation = "WestEurope";
    private const int MaxServiceConnectionLength = 200;
    private static readonly Guid ValidResourceGroupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ValidContainerAppEnvironmentId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new CreateContainerAppRequest
        {
            Name = "ca-api",
            Location = ValidLocation,
            ResourceGroupId = ValidResourceGroupId,
            ContainerAppEnvironmentId = ValidContainerAppEnvironmentId,
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
        var sut = new CreateContainerAppRequest
        {
            Name = null!,
            Location = ValidLocation,
            ResourceGroupId = ValidResourceGroupId,
            ContainerAppEnvironmentId = ValidContainerAppEnvironmentId,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateContainerAppRequest.Name)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullLocation_When_Validate_Then_Error()
    {
        // Arrange
        var sut = new CreateContainerAppRequest
        {
            Name = "ca-api",
            Location = null!,
            ResourceGroupId = ValidResourceGroupId,
            ContainerAppEnvironmentId = ValidContainerAppEnvironmentId,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateContainerAppRequest.Location)).Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidLocation_When_Validate_Then_Error()
    {
        // Arrange
        var sut = new CreateContainerAppRequest
        {
            Name = "ca-api",
            Location = "InvalidRegion",
            ResourceGroupId = ValidResourceGroupId,
            ContainerAppEnvironmentId = ValidContainerAppEnvironmentId,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateContainerAppRequest.Location)).Should().BeTrue();
    }

    [Fact]
    public void Given_EmptyResourceGroupId_When_Validate_Then_Error()
    {
        // Arrange
        var sut = new CreateContainerAppRequest
        {
            Name = "ca-api",
            Location = ValidLocation,
            ResourceGroupId = Guid.Empty,
            ContainerAppEnvironmentId = ValidContainerAppEnvironmentId,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateContainerAppRequest.ResourceGroupId)).Should().BeTrue();
    }

    [Fact]
    public void Given_EmptyContainerAppEnvironmentId_When_Validate_Then_Error()
    {
        // Arrange
        var sut = new CreateContainerAppRequest
        {
            Name = "ca-api",
            Location = ValidLocation,
            ResourceGroupId = ValidResourceGroupId,
            ContainerAppEnvironmentId = Guid.Empty,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateContainerAppRequest.ContainerAppEnvironmentId)).Should().BeTrue();
    }

    [Fact]
    public void Given_TooLongEnvironmentContainerRegistryServiceConnection_When_ValidateEntry_Then_Error()
    {
        // Arrange
        var sut = new ContainerAppEnvironmentConfigEntry
        {
            EnvironmentName = "dev",
            ContainerRegistryServiceConnection = new string('a', MaxServiceConnectionLength + 1),
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(ContainerAppEnvironmentConfigEntry.ContainerRegistryServiceConnection)).Should().BeTrue();
    }
}
