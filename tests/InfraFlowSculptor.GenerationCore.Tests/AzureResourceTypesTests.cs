using System.Collections.Frozen;
using FluentAssertions;

namespace InfraFlowSculptor.GenerationCore.Tests;

public sealed class AzureResourceTypesTests
{
    [Fact]
    public void Given_KnownArmResourceType_When_GetFriendlyName_Then_ReturnsFriendlyType()
    {
        // Act
        var result = AzureResourceTypes.GetFriendlyName(AzureResourceTypes.ArmTypes.WebAppType);

        // Assert
        result.Should().Be(AzureResourceTypes.WebApp);
    }

    [Fact]
    public void Given_MixedCaseArmResourceType_When_GetFriendlyName_Then_ReturnsFriendlyType()
    {
        // Act
        var result = AzureResourceTypes.GetFriendlyName(AzureResourceTypes.ArmTypes.ContainerAppType.ToUpperInvariant());

        // Assert
        result.Should().Be(AzureResourceTypes.ContainerApp);
    }

    [Fact]
    public void Given_UnknownArmResourceType_When_GetFriendlyName_Then_ReturnsOriginalType()
    {
        // Arrange
        const string unknownArmType = "Microsoft.Custom/widgets";

        // Act
        var result = AzureResourceTypes.GetFriendlyName(unknownArmType);

        // Assert
        result.Should().Be(unknownArmType);
    }

    [Fact]
    public void Given_PublicArmTypeMap_When_Inspected_Then_UsesFrozenDictionary()
    {
        AzureResourceTypes.ArmTypeToFriendlyName.Should().BeAssignableTo<FrozenDictionary<string, string>>();
    }

    [Fact]
    public void Given_ComputeArmTypes_When_Inspected_Then_ContainsRenamedArmTypeConstants()
    {
        AzureResourceTypes.ComputeArmTypes.Should().Contain(
        [
            AzureResourceTypes.ArmTypes.WebAppType,
            AzureResourceTypes.ArmTypes.FunctionAppType,
            AzureResourceTypes.ArmTypes.ContainerAppType,
        ]);
    }
}
