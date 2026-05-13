using System.Reflection;
using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.Common.BaseModels;

public sealed class AzureResourceTests
{
    private const string DefaultName = "kv-prod-data";
    private const string ExplicitOverrideName = "kv-shared-prod-001";

    [Fact]
    public void Given_AzureResourceBaseProperties_When_InspectingSetterVisibility_Then_SettersAreNotPublic()
    {
        // Arrange
        var sutType = typeof(AzureResource);

        // Act
        var resourceGroupIdSetter = sutType.GetProperty(nameof(AzureResource.ResourceGroupId))?.SetMethod;
        var resourceGroupSetter = sutType.GetProperty(nameof(AzureResource.ResourceGroup))?.SetMethod;
        var nameSetter = sutType.GetProperty(nameof(AzureResource.Name))?.SetMethod;
        var locationSetter = sutType.GetProperty(nameof(AzureResource.Location))?.SetMethod;
        var customNameOverrideSetter = sutType.GetProperty(nameof(AzureResource.CustomNameOverride))?.SetMethod;

        // Assert
        resourceGroupIdSetter.Should().NotBeNull();
        resourceGroupIdSetter!.IsPublic.Should().BeFalse();
        resourceGroupSetter.Should().NotBeNull();
        resourceGroupSetter!.IsPublic.Should().BeFalse();
        nameSetter.Should().NotBeNull();
        nameSetter!.IsPublic.Should().BeFalse();
        locationSetter.Should().NotBeNull();
        locationSetter!.IsPublic.Should().BeFalse();
        customNameOverrideSetter.Should().NotBeNull();
        customNameOverrideSetter!.IsPublic.Should().BeFalse();
    }

    [Fact]
    public void Given_ValidName_When_RenameIsInvoked_Then_UpdatesName()
    {
        // Arrange
        var sut = CreateValidKeyVault();
        var newName = new Name("kv-renamed");
        var renameMethod = typeof(AzureResource).GetMethod(
            "Rename",
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            types: [typeof(Name)],
            modifiers: null);

        // Act
        renameMethod.Should().NotBeNull();
        renameMethod!.Invoke(sut, [newName]);

        // Assert
        sut.Name.Should().Be(newName);
    }

    [Fact]
    public void Given_DifferentResourceGroup_When_MoveToResourceGroupIsInvoked_Then_UpdatesResourceGroupId()
    {
        // Arrange
        var sut = CreateValidKeyVault();
        var newResourceGroupId = ResourceGroupId.CreateUnique();
        var moveToResourceGroupMethod = typeof(AzureResource).GetMethod(
            "MoveToResourceGroup",
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            types: [typeof(ResourceGroupId)],
            modifiers: null);

        // Act
        moveToResourceGroupMethod.Should().NotBeNull();
        moveToResourceGroupMethod!.Invoke(sut, [newResourceGroupId]);

        // Assert
        sut.ResourceGroupId.Should().Be(newResourceGroupId);
    }

    [Fact]
    public void Given_ExplicitNameOverride_When_OverrideNameIsInvoked_Then_StoresAndClearsOverride()
    {
        // Arrange
        var sut = CreateValidKeyVault();
        var overrideNameMethod = typeof(AzureResource).GetMethod(
            "OverrideName",
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            types: [typeof(string)],
            modifiers: null);
        var clearOverrideNameMethod = typeof(AzureResource).GetMethod(
            "ClearNameOverride",
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            types: Type.EmptyTypes,
            modifiers: null);

        // Act
        overrideNameMethod.Should().NotBeNull();
        overrideNameMethod!.Invoke(sut, [ExplicitOverrideName]);
        sut.CustomNameOverride.Should().Be(ExplicitOverrideName);
        clearOverrideNameMethod.Should().NotBeNull();
        clearOverrideNameMethod!.Invoke(sut, []);

        // Assert
        sut.CustomNameOverride.Should().BeNull();
    }

    private static KeyVault CreateValidKeyVault()
    {
        return KeyVault.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(Location.LocationEnum.WestEurope));
    }
}