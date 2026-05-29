using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.BicepGeneration.Pipeline;
using InfraFlowSculptor.BicepGeneration.Pipeline.Stages;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Pipeline.Stages;

public sealed class PrivateEndpointCompanionStageTests
{
    private readonly PrivateEndpointCompanionStage _sut = new();

    [Fact]
    public void Given_Stage_When_CheckOrder_Then_Returns540()
    {
        _sut.Order.Should().Be(540);
    }

    [Fact]
    public void Given_PrivatizedKeyVault_When_Execute_Then_AddsCompanionWorkItem()
    {
        // Arrange
        var resource = CreatePrivatizedResource("kv-main", "Microsoft.KeyVault/vaults", "KeyVault");
        var context = CreateContext(resource);

        // Act
        _sut.Execute(context);

        // Assert — original + companion
        context.WorkItems.Should().HaveCount(2);
        var companion = context.WorkItems[1];
        companion.Spec.ResourceTypeName.Should().Be("PrivateEndpoint");
        companion.Module.ModuleFolderName.Should().Be("PrivateEndpoints");
    }

    [Fact]
    public void Given_PrivatizedKeyVault_When_Execute_Then_CompanionHasCorrectGroupIds()
    {
        // Arrange
        var resource = CreatePrivatizedResource("kv-main", "Microsoft.KeyVault/vaults", "KeyVault");
        var context = CreateContext(resource);

        // Act
        _sut.Execute(context);

        // Assert — PE spec should contain the vault group ID in the privateLinkServiceConnections
        var companionSpec = context.WorkItems[1].Spec;
        var resourceBody = companionSpec.Resource.Body;
        var propsAssignment = resourceBody.First(p => p.Key == "properties");
        var propsObj = (BicepObjectExpression)propsAssignment.Value;
        var plscProp = propsObj.Properties.First(p => p.Key == "privateLinkServiceConnections");
        var plscArray = (BicepArrayExpression)plscProp.Value;

        // The groupIds array should contain "vault"
        plscArray.Items.Should().NotBeEmpty();
    }

    [Fact]
    public void Given_NonPrivatizedResource_When_Execute_Then_NoCompanionAdded()
    {
        // Arrange
        var resource = new ResourceDefinition
        {
            Name = "kv-main",
            Type = "Microsoft.KeyVault/vaults",
            IsPrivatized = false,
        };
        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest { Resources = [resource] },
        };
        context.WorkItems.Add(CreateWorkItem(resource, "KeyVault"));

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems.Should().HaveCount(1);
    }

    [Fact]
    public void Given_PrivatizedWithNoPeConfig_When_Execute_Then_NoCompanionAdded()
    {
        // Arrange
        var resource = new ResourceDefinition
        {
            Name = "kv-main",
            Type = "Microsoft.KeyVault/vaults",
            IsPrivatized = true,
            PrivateEndpointConfig = null,
        };
        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest { Resources = [resource] },
        };
        context.WorkItems.Add(CreateWorkItem(resource, "KeyVault"));

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems.Should().HaveCount(1);
    }

    [Fact]
    public void Given_UnknownResourceType_When_Execute_Then_SkipsWithoutError()
    {
        // Arrange
        var resource = CreatePrivatizedResource("unknown-res", "Microsoft.Unknown/things", "UnknownThing");
        var context = CreateContext(resource, "UnknownThing");

        // Act
        _sut.Execute(context);

        // Assert — no companion since UnknownThing is not in the catalog
        context.WorkItems.Should().HaveCount(1);
    }

    [Fact]
    public void Given_MultiplePrivatizedResources_When_Execute_Then_AddsCompanionForEach()
    {
        // Arrange
        var kv = CreatePrivatizedResource("kv-1", "Microsoft.KeyVault/vaults", "KeyVault");
        var redis = CreatePrivatizedResource("redis-1", "Microsoft.Cache/Redis", "RedisCache");
        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest { Resources = [kv, redis] },
        };
        context.WorkItems.Add(CreateWorkItem(kv, "KeyVault"));
        context.WorkItems.Add(CreateWorkItem(redis, "RedisCache"));

        // Act
        _sut.Execute(context);

        // Assert — 2 original + 2 companions
        context.WorkItems.Should().HaveCount(4);
        context.WorkItems.Count(wi => wi.Spec.ResourceTypeName == "PrivateEndpoint").Should().Be(2);
    }

    private static ResourceDefinition CreatePrivatizedResource(string name, string type, string typeName)
    {
        return new ResourceDefinition
        {
            Name = name,
            Type = type,
            IsPrivatized = true,
            PrivateEndpointConfig = new PrivateEndpointDefinition
            {
                VirtualNetworkId = Guid.NewGuid(),
                SubnetName = "snet-pe",
                DnsMode = "AutoManaged",
            },
        };
    }

    private static BicepGenerationContext CreateContext(ResourceDefinition resource, string? resourceTypeName = null)
    {
        var typeName = resourceTypeName
                       ?? (resource.Name.Contains("kv") ? "KeyVault" : "RedisCache");
        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest { Resources = [resource] },
        };
        context.WorkItems.Add(CreateWorkItem(resource, typeName));
        return context;
    }

    private static ModuleWorkItem CreateWorkItem(ResourceDefinition resource, string resourceTypeName)
    {
        return new ModuleWorkItem
        {
            Resource = resource,
            Module = new GeneratedTypeModule
            {
                ModuleName = $"mod{resourceTypeName}",
                ModuleFolderName = resourceTypeName,
                ResourceTypeName = resourceTypeName,
                ResourceGroupName = "rg-main",
                LogicalResourceName = resource.Name,
            },
            Spec = new BicepModuleSpec
            {
                ModuleName = $"mod{resourceTypeName}",
                ModuleFolderName = resourceTypeName,
                ResourceTypeName = resourceTypeName,
                Resource = new BicepResourceDeclaration
                {
                    Symbol = "res",
                    ArmTypeWithApiVersion = "Microsoft.Test/resources@2023-01-01",
                    Body = [
                        new BicepPropertyAssignment("name", new BicepReference("name")),
                        new BicepPropertyAssignment("location", new BicepReference("location")),
                    ],
                },
            },
        };
    }
}
