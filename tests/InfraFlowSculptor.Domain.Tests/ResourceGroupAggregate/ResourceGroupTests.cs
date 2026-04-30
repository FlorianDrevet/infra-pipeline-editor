using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.ResourceGroupAggregate;

public sealed class ResourceGroupTests
{
    private const string DefaultGroupName = "rg-prod";
    private const Location.LocationEnum DefaultLocationValue = Location.LocationEnum.WestEurope;

    private static ResourceGroup CreateValidResourceGroup(
        Location.LocationEnum location = DefaultLocationValue)
    {
        return ResourceGroup.Create(
            new Name(DefaultGroupName),
            InfrastructureConfigId.CreateUnique(),
            new Location(location));
    }

    private static StorageAccount CreateStorageAccount(
        ResourceGroupId groupId,
        string name,
        Location.LocationEnum location = DefaultLocationValue)
    {
        return StorageAccount.Create(
            groupId,
            new Name(name),
            new Location(location),
            new StorageAccountKind(StorageAccountKind.Kind.StorageV2),
            new StorageAccessTier(StorageAccessTier.Tier.Hot),
            allowBlobPublicAccess: false,
            enableHttpsTrafficOnly: true,
            new StorageAccountTlsVersion(StorageAccountTlsVersion.Version.Tls12));
    }

    // ─── Factory ────────────────────────────────────────────────────────────

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var name = new Name(DefaultGroupName);
        var infraConfigId = InfrastructureConfigId.CreateUnique();
        var location = new Location(DefaultLocationValue);

        // Act
        var sut = ResourceGroup.Create(name, infraConfigId, location);

        // Assert
        sut.Id.Should().NotBeNull();
        sut.Id.Value.Should().NotBe(Guid.Empty);
        sut.Name.Should().Be(name);
        sut.InfraConfigId.Should().Be(infraConfigId);
        sut.Location.Should().Be(location);
        sut.Resources.Should().BeEmpty();
    }

    // ─── AddResource ────────────────────────────────────────────────────────

    [Fact]
    public void Given_ResourceInSameLocation_When_AddResource_Then_ReturnsSuccessAndAdds()
    {
        // Arrange
        var sut = CreateValidResourceGroup();
        var resource = CreateStorageAccount(sut.Id, "stuniqueone");

        // Act
        var result = sut.AddResource(resource);

        // Assert
        result.IsError.Should().BeFalse();
        sut.Resources.Should().ContainSingle().Which.Should().BeSameAs(resource);
    }

    [Fact]
    public void Given_ResourceInDifferentLocation_When_AddResource_Then_ReturnsLocationError()
    {
        // Arrange
        var sut = CreateValidResourceGroup(Location.LocationEnum.WestEurope);
        var resource = CreateStorageAccount(sut.Id, "stuniqueone", Location.LocationEnum.EastUS);

        // Act
        var result = sut.AddResource(resource);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.ResourceGroup.AddResource.ErrorCodes.ResourceNotInSameLocationCode);
        sut.Resources.Should().BeEmpty();
    }

    [Fact]
    public void Given_DuplicateResourceName_When_AddResource_Then_ReturnsAlreadyInGroupError()
    {
        // Arrange
        var sut = CreateValidResourceGroup();
        var first = CreateStorageAccount(sut.Id, "stshared");
        var duplicate = CreateStorageAccount(sut.Id, "stshared");
        sut.AddResource(first);

        // Act
        var result = sut.AddResource(duplicate);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.ResourceGroup.AddResource.ErrorCodes.ResourceAlreadyInGroupCode);
        sut.Resources.Should().ContainSingle();
    }

    [Fact]
    public void Given_GroupAtCapacity_When_AddResource_Then_ReturnsLimitReachedError()
    {
        // Arrange
        const int azureResourceGroupResourceLimit = 800;
        var sut = CreateValidResourceGroup();
        for (var i = 0; i < azureResourceGroupResourceLimit; i++)
        {
            var fillResource = CreateStorageAccount(sut.Id, $"stfill{i:D4}");
            sut.AddResource(fillResource);
        }
        var extra = CreateStorageAccount(sut.Id, "stoverflow");

        // Act
        var result = sut.AddResource(extra);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.ResourceGroup.AddResource.ErrorCodes.ResourceGroupResourceLimitReachedCode);
        sut.Resources.Should().HaveCount(azureResourceGroupResourceLimit);
    }

    // ─── RemoveResource ─────────────────────────────────────────────────────

    [Fact]
    public void Given_ExistingResource_When_RemoveResource_Then_ReturnsSuccessAndRemoves()
    {
        // Arrange
        var sut = CreateValidResourceGroup();
        var resource = CreateStorageAccount(sut.Id, "stremovable");
        sut.AddResource(resource);

        // Act
        var result = sut.RemoveResource(resource);

        // Assert
        result.IsError.Should().BeFalse();
        sut.Resources.Should().BeEmpty();
    }

    [Fact]
    public void Given_NonMemberResource_When_RemoveResource_Then_ReturnsNotInGroupError()
    {
        // Arrange
        var sut = CreateValidResourceGroup();
        var stranger = CreateStorageAccount(sut.Id, "ststranger");

        // Act
        var result = sut.RemoveResource(stranger);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.ResourceGroup.RemoveResource.ErrorCodes.ResourceNotInGroupCode);
    }

    [Fact]
    public void Given_ResourceUsedAsDependency_When_RemoveResource_Then_ReturnsIsDependencyError()
    {
        // Arrange
        var sut = CreateValidResourceGroup();
        var dependency = CreateStorageAccount(sut.Id, "stdependency");
        var dependent = CreateStorageAccount(sut.Id, "stdependent");
        sut.AddResource(dependency);
        sut.AddResource(dependent);
        dependent.AddDependency(dependency);

        // Act
        var result = sut.RemoveResource(dependency);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.ResourceGroup.RemoveResource.ErrorCodes.ResourceIsDependencyCode);
        sut.Resources.Should().Contain(dependency);
    }
}
