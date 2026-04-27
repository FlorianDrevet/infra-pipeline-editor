using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.BicepGeneration.Pipeline;
using InfraFlowSculptor.BicepGeneration.Pipeline.Stages;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Pipeline.Stages;

public sealed class IdentityInjectionStageTests
{
    private readonly IdentityInjectionStage _sut = new();

    [Fact]
    public void Given_Stage_When_CheckOrder_Then_Returns400()
    {
        _sut.Order.Should().Be(400);
    }

    [Fact]
    public void Given_ResourceWithSystemIdentity_When_Execute_Then_SystemAssignedInjected()
    {
        // Arrange
        var context = CreateContextWithWorkItem(
            "my-webapp", "Microsoft.Web/sites",
            systemIdentity: true, userIdentity: false, mixed: false);

        // Act
        _sut.Execute(context);

        // Assert
        var item = context.WorkItems[0];
        item.IdentityKind.Should().Be("SystemAssigned");
        item.UsesParameterizedIdentity.Should().BeFalse();
        item.Spec.Resource.Body.Should().Contain(p => p.Key == "identity");
    }

    [Fact]
    public void Given_ResourceWithUserIdentity_When_Execute_Then_UserAssignedInjected()
    {
        // Arrange
        var context = CreateContextWithWorkItem(
            "my-webapp", "Microsoft.Web/sites",
            systemIdentity: false, userIdentity: true, mixed: false);

        // Act
        _sut.Execute(context);

        // Assert
        var item = context.WorkItems[0];
        item.IdentityKind.Should().Be("UserAssigned");
        item.Spec.Resource.Body.Should().Contain(p => p.Key == "identity");
        item.Spec.Parameters.Should().Contain(p => p.Name == "userAssignedIdentityId");
    }

    [Fact]
    public void Given_ResourceWithBothIdentities_When_Execute_Then_CombinedKind()
    {
        // Arrange
        var context = CreateContextWithWorkItem(
            "my-webapp", "Microsoft.Web/sites",
            systemIdentity: true, userIdentity: true, mixed: false);

        // Act
        _sut.Execute(context);

        // Assert
        var item = context.WorkItems[0];
        item.IdentityKind.Should().Be("SystemAssigned, UserAssigned");
    }

    [Fact]
    public void Given_MixedArmType_When_Execute_Then_ParameterizedIdentityInjected()
    {
        // Arrange
        var context = CreateContextWithWorkItem(
            "my-webapp", "Microsoft.Web/sites",
            systemIdentity: true, userIdentity: false, mixed: true);

        // Act
        _sut.Execute(context);

        // Assert
        var item = context.WorkItems[0];
        item.UsesParameterizedIdentity.Should().BeTrue();
        item.Spec.Parameters.Should().Contain(p => p.Name == "identityType");
        item.Spec.ExportedTypes.Should().Contain(t => t.Name == "ManagedIdentityType");
    }

    [Fact]
    public void Given_ResourceWithNoIdentity_When_Execute_Then_IdentityKindIsNull()
    {
        // Arrange
        var context = CreateContextWithWorkItem(
            "my-kv", "Microsoft.KeyVault/vaults",
            systemIdentity: false, userIdentity: false, mixed: false);

        // Act
        _sut.Execute(context);

        // Assert
        var item = context.WorkItems[0];
        item.IdentityKind.Should().BeNull();
        item.UsesParameterizedIdentity.Should().BeFalse();
    }

    [Fact]
    public void Given_UaiResourceType_When_Execute_Then_UserIdentitySkipped()
    {
        // Arrange — UAI resource type is excluded from user-identity injection
        var resource = new ResourceDefinition
        {
            ResourceId = Guid.NewGuid(),
            Name = "my-uai",
            Type = "Microsoft.ManagedIdentity/userAssignedIdentities",
        };
        var identity = new IdentityAnalysisResult(
            SystemIdentityResources: [],
            UserIdentityResources: new Dictionary<(string, string), List<string>>
            {
                [("my-uai", "Microsoft.ManagedIdentity/userAssignedIdentities")] = ["myUaiBicep"],
            },
            MixedIdentityArmTypes: []);

        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest { Resources = [resource] },
            Identity = identity,
        };
        context.WorkItems.Add(new ModuleWorkItem
        {
            Resource = resource,
            Module = new GeneratedTypeModule(),
            Spec = CreateMinimalSpec(),
        });

        // Act
        _sut.Execute(context);

        // Assert — needsUser is false because resource type is UAI itself
        var item = context.WorkItems[0];
        item.IdentityKind.Should().BeNull();
    }

    // ── Helpers ──

    private static BicepModuleSpec CreateMinimalSpec() => new()
    {
        ModuleName = "test",
        ModuleFolderName = "Test",
        ResourceTypeName = "Test",
        Resource = new BicepResourceDeclaration
        {
            Symbol = "testResource",
            ArmTypeWithApiVersion = "Microsoft.Test/resources@2024-01-01",
        },
    };

    private static BicepGenerationContext CreateContextWithWorkItem(
        string resourceName, string armType,
        bool systemIdentity, bool userIdentity, bool mixed)
    {
        var resource = new ResourceDefinition
        {
            ResourceId = Guid.NewGuid(),
            Name = resourceName,
            Type = armType,
        };

        var systemSet = new HashSet<(string, string)>();
        if (systemIdentity) systemSet.Add((resourceName, armType));

        var userDict = new Dictionary<(string, string), List<string>>();
        if (userIdentity) userDict[(resourceName, armType)] = ["myUaiBicep"];

        var mixedSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (mixed) mixedSet.Add(armType);

        var identity = new IdentityAnalysisResult(systemSet, userDict, mixedSet);

        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest { Resources = [resource] },
            Identity = identity,
        };
        context.WorkItems.Add(new ModuleWorkItem
        {
            Resource = resource,
            Module = new GeneratedTypeModule(),
            Spec = CreateMinimalSpec(),
        });

        return context;
    }
}
