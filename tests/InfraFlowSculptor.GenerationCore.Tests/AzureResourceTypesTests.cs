using System.Collections.Frozen;
using FluentAssertions;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.GenerationCore.Tests;

public sealed class AzureResourceTypesTests
{
    [Fact]
    public void Given_KnownArmResourceType_When_GetFriendlyName_Then_ReturnsFriendlyType()
    {
        // Act
        var result = AzureResourceTypes.GetFriendlyName(AzureResourceTypes.ArmTypes.WebApp);

        // Assert
        result.Should().Be(AzureResourceTypes.WebApp);
    }

    [Fact]
    public void Given_MixedCaseArmResourceType_When_GetFriendlyName_Then_ReturnsFriendlyType()
    {
        // Act
        var result = AzureResourceTypes.GetFriendlyName(AzureResourceTypes.ArmTypes.ContainerApp.ToUpperInvariant());

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
        AzureResourceTypes.ArmTypeToFriendlyName.Should().BeOfType<FrozenDictionary<string, string>>();
    }
}