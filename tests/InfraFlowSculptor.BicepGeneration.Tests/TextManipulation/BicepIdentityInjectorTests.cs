using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.TextManipulation;

namespace InfraFlowSculptor.BicepGeneration.Tests.TextManipulation;

public sealed class BicepIdentityInjectorTests
{
    // ── Minimal Bicep template fixtures ──

    private const string MinimalModule = """
        param location string

        resource kv 'Microsoft.KeyVault/vaults' = {
          name: 'myKv'
          location: location
          properties: {
            sku: {
              family: 'A'
              name: 'standard'
            }
          }
        }
        """;

    private const string ModuleWithExistingIdentity = """
        param location string

        resource kv 'Microsoft.KeyVault/vaults' = {
          name: 'myKv'
          location: location
          identity: {
            type: 'SystemAssigned'
          }
          properties: {
            sku: { family: 'A' }
          }
        }
        """;

    private const string ModuleWithExistingPrincipalIdOutput = """
        param location string

        resource kv 'Microsoft.KeyVault/vaults' = {
          name: 'myKv'
          location: location
          properties: {}
        }

        output principalId string = kv.identity.principalId
        """;

    // ── InjectSystemAssigned ──

    [Fact]
    public void Given_ModuleWithoutIdentity_When_InjectSystemAssigned_Then_AddsIdentityBlockAndPrincipalIdOutput()
    {
        // Act
        var result = BicepIdentityInjector.InjectSystemAssigned(MinimalModule);

        // Assert
        result.Should().Contain("identity: {");
        result.Should().Contain("type: 'SystemAssigned'");
        result.Should().Contain("output principalId string = kv.identity.principalId");
    }

    [Fact]
    public void Given_ModuleWithExistingIdentity_When_InjectSystemAssigned_Then_NoOp()
    {
        // Act
        var result = BicepIdentityInjector.InjectSystemAssigned(ModuleWithExistingIdentity);

        // Assert
        result.Should().Be(ModuleWithExistingIdentity);
    }

    [Fact]
    public void Given_ModuleWithExistingPrincipalIdOutput_When_InjectSystemAssigned_Then_NoOp()
    {
        // Act
        var result = BicepIdentityInjector.InjectSystemAssigned(ModuleWithExistingPrincipalIdOutput);

        // Assert
        result.Should().Be(ModuleWithExistingPrincipalIdOutput);
    }

    [Fact]
    public void Given_ModuleWithNoResourceSymbol_When_InjectSystemAssigned_Then_ReturnsUnchanged()
    {
        // Arrange
        const string bicep = "param location string\n// no resource declaration";

        // Act
        var result = BicepIdentityInjector.InjectSystemAssigned(bicep);

        // Assert
        result.Should().Be(bicep);
    }

    // ── InjectUserAssigned ──

    [Fact]
    public void Given_ModuleWithoutIdentity_When_InjectUserAssigned_Then_AddsUserAssignedBlock()
    {
        // Arrange
        var uaiIdentifiers = new List<string> { "myUai" };

        // Act
        var result = BicepIdentityInjector.InjectUserAssigned(MinimalModule, uaiIdentifiers, alsoHasSystemAssigned: false);

        // Assert
        result.Should().Contain("type: 'UserAssigned'");
        result.Should().Contain("userAssignedIdentities:");
        result.Should().Contain("param userAssignedIdentityId string");
    }

    [Fact]
    public void Given_ModuleWithSystemIdentity_When_InjectUserAssigned_AlsoHasSystem_Then_UpgradesToCombined()
    {
        // Arrange
        var uaiIdentifiers = new List<string> { "myUai" };

        // Act
        var result = BicepIdentityInjector.InjectUserAssigned(
            ModuleWithExistingIdentity, uaiIdentifiers, alsoHasSystemAssigned: true);

        // Assert
        result.Should().Contain("type: 'SystemAssigned, UserAssigned'");
        result.Should().Contain("userAssignedIdentities:");
    }

    [Fact]
    public void Given_ModuleAlreadyHasUserAssignedParam_When_InjectUserAssigned_Then_DoesNotDuplicateParam()
    {
        // Arrange
        const string bicep = """
            param location string
            param userAssignedIdentityId string

            resource kv 'Microsoft.KeyVault/vaults' = {
              name: 'myKv'
              location: location
              properties: {}
            }
            """;
        var uaiIdentifiers = new List<string> { "myUai" };

        // Act
        var result = BicepIdentityInjector.InjectUserAssigned(bicep, uaiIdentifiers, alsoHasSystemAssigned: false);

        // Assert — param should appear exactly once
        var paramCount = result.Split("param userAssignedIdentityId").Length - 1;
        paramCount.Should().Be(1);
    }

    // ── InjectParameterized ──

    [Fact]
    public void Given_ModuleWithoutIdentity_When_InjectParameterized_Then_AddsParameterizedIdentityBlock()
    {
        // Act
        var result = BicepIdentityInjector.InjectParameterized(MinimalModule, hasAnyUai: false);

        // Assert
        result.Should().Contain("param identityType ManagedIdentityType");
        result.Should().Contain("identity:");
        result.Should().Contain("type: identityType");
        result.Should().Contain("import { ManagedIdentityType } from './types.bicep'");
    }

    [Fact]
    public void Given_ModuleWithoutIdentity_When_InjectParameterized_HasUai_Then_AddsUaiParam()
    {
        // Act
        var result = BicepIdentityInjector.InjectParameterized(MinimalModule, hasAnyUai: true);

        // Assert
        result.Should().Contain("param identityType ManagedIdentityType");
        result.Should().Contain("param userAssignedIdentityId string");
        result.Should().Contain("contains(identityType, 'UserAssigned')");
    }

    [Fact]
    public void Given_ModuleWithExistingIdentityBlock_When_InjectParameterized_Then_NoOp()
    {
        // Act
        var result = BicepIdentityInjector.InjectParameterized(ModuleWithExistingIdentity, hasAnyUai: false);

        // Assert
        result.Should().Be(ModuleWithExistingIdentity);
    }

    [Fact]
    public void Given_ModuleWithExistingIdentityTypeParam_When_InjectParameterized_Then_NoOp()
    {
        // Arrange
        const string bicep = """
            param identityType string = 'SystemAssigned'
            param location string

            resource kv 'Microsoft.KeyVault/vaults' = {
              name: 'myKv'
              location: location
              properties: {}
            }
            """;

        // Act
        var result = BicepIdentityInjector.InjectParameterized(bicep, hasAnyUai: false);

        // Assert
        result.Should().Be(bicep);
    }

    [Fact]
    public void Given_NoResourceSymbol_When_InjectParameterized_Then_ReturnsUnchanged()
    {
        // Arrange
        const string bicep = "param location string\n// no resource";

        // Act
        var result = BicepIdentityInjector.InjectParameterized(bicep, hasAnyUai: false);

        // Assert
        result.Should().Be(bicep);
    }

    // ── ManagedIdentityTypeBicepType constant ──

    [Fact]
    public void Given_ManagedIdentityTypeBicepType_Then_ContainsExpectedTypeDefinition()
    {
        // Assert
        BicepIdentityInjector.ManagedIdentityTypeBicepType.Should().Contain("@export()");
        BicepIdentityInjector.ManagedIdentityTypeBicepType.Should().Contain("type ManagedIdentityType");
        BicepIdentityInjector.ManagedIdentityTypeBicepType.Should().Contain("'SystemAssigned'");
        BicepIdentityInjector.ManagedIdentityTypeBicepType.Should().Contain("'UserAssigned'");
    }
}
