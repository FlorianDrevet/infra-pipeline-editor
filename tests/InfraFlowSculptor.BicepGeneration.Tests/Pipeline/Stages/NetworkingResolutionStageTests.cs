using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.BicepGeneration.Pipeline;
using InfraFlowSculptor.BicepGeneration.Pipeline.Stages;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Pipeline.Stages;

public sealed class NetworkingResolutionStageTests
{
    private readonly NetworkingResolutionStage _sut = new();

    [Fact]
    public void Given_Stage_When_CheckOrder_Then_Returns520()
    {
        _sut.Order.Should().Be(520);
    }

    [Fact]
    public void Given_PrivatizedResource_When_Execute_Then_AddsNetworkingResolutionWorkItem()
    {
        // Arrange
        var resource = CreatePrivatizedResource("kv-1");
        var context = CreateContext(resource);

        // Act
        _sut.Execute(context);

        // Assert — original + networking resolution module
        context.WorkItems.Should().HaveCount(2);
        var networkingItem = context.WorkItems[1];
        networkingItem.Module.ModuleFolderName.Should().Be("Networking");
        networkingItem.Spec.ResourceTypeName.Should().Be("NetworkingResolution");
    }

    [Fact]
    public void Given_PrivatizedResource_When_Execute_Then_ResolutionHasVnetAndSubnetParams()
    {
        // Arrange
        var resource = CreatePrivatizedResource("kv-1");
        var context = CreateContext(resource);

        // Act
        _sut.Execute(context);

        // Assert
        var spec = context.WorkItems[1].Spec;
        spec.Parameters.Should().Contain(p => p.Name == "vnetResourceId");
        spec.Parameters.Should().Contain(p => p.Name == "peSubnetName");
    }

    [Fact]
    public void Given_PrivatizedResource_When_Execute_Then_ResolutionOutputsSubnetId()
    {
        // Arrange
        var resource = CreatePrivatizedResource("kv-1");
        var context = CreateContext(resource);

        // Act
        _sut.Execute(context);

        // Assert
        var spec = context.WorkItems[1].Spec;
        spec.Outputs.Should().Contain(o => o.Name == "peSubnetId");
    }

    [Fact]
    public void Given_NoPrivatizedResources_When_Execute_Then_NoWorkItemAdded()
    {
        // Arrange
        var resource = new ResourceDefinition
        {
            Name = "kv-1",
            Type = "Microsoft.KeyVault/vaults",
            IsPrivatized = false,
        };
        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest { Resources = [resource] },
        };
        context.WorkItems.Add(CreateWorkItem(resource));

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems.Should().HaveCount(1);
    }

    [Fact]
    public void Given_PrivatizedWithNoPeConfig_When_Execute_Then_NoWorkItemAdded()
    {
        // Arrange
        var resource = new ResourceDefinition
        {
            Name = "kv-1",
            Type = "Microsoft.KeyVault/vaults",
            IsPrivatized = true,
            PrivateEndpointConfig = null,
        };
        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest { Resources = [resource] },
        };
        context.WorkItems.Add(CreateWorkItem(resource));

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems.Should().HaveCount(1);
    }

    private static ResourceDefinition CreatePrivatizedResource(string name)
    {
        return new ResourceDefinition
        {
            Name = name,
            Type = "Microsoft.KeyVault/vaults",
            IsPrivatized = true,
            PrivateEndpointConfig = new PrivateEndpointDefinition
            {
                VirtualNetworkId = Guid.NewGuid(),
                SubnetName = "snet-pe",
                DnsMode = "AutoManaged",
            },
        };
    }

    private static BicepGenerationContext CreateContext(ResourceDefinition resource)
    {
        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest { Resources = [resource] },
        };
        context.WorkItems.Add(CreateWorkItem(resource));
        return context;
    }

    private static ModuleWorkItem CreateWorkItem(ResourceDefinition resource)
    {
        return new ModuleWorkItem
        {
            Resource = resource,
            Module = new GeneratedTypeModule(),
            Spec = new BicepModuleSpec
            {
                ModuleName = "test",
                ModuleFolderName = "Test",
                ResourceTypeName = "Test",
                Resource = new BicepResourceDeclaration
                {
                    Symbol = "res",
                    ArmTypeWithApiVersion = "Microsoft.Test/resources@2023-01-01",
                    Body = [
                        new BicepPropertyAssignment("name", new BicepReference("name")),
                    ],
                },
            },
        };
    }
}
