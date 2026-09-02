using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.BicepGeneration.Pipeline;
using InfraFlowSculptor.BicepGeneration.Pipeline.Stages;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Pipeline.Stages;

public sealed class PublicNetworkAccessStageTests
{
    private readonly PublicNetworkAccessStage _sut = new();

    [Fact]
    public void Given_Stage_When_CheckOrder_Then_Returns560()
    {
        _sut.Order.Should().Be(560);
    }

    [Fact]
    public void Given_PrivatizedKeyVault_When_Execute_Then_InjectsPublicNetworkAccessAndNetworkAcls()
    {
        // Arrange
        var context = CreateContextWithPrivatizedResource("KeyVault");

        // Act
        _sut.Execute(context);

        // Assert
        var spec = context.WorkItems[0].Spec;
        var propsAssignment = spec.Resource.Body.First(p => p.Key == "properties");
        var propsObj = propsAssignment.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        propsObj.Properties.Should().Contain(p => p.Key == "publicNetworkAccess");
        propsObj.Properties.Should().Contain(p => p.Key == "networkAcls");
    }

    [Fact]
    public void Given_PrivatizedRedis_When_Execute_Then_InjectsOnlyPublicNetworkAccess()
    {
        // Arrange
        var context = CreateContextWithPrivatizedResource("RedisCache");

        // Act
        _sut.Execute(context);

        // Assert
        var spec = context.WorkItems[0].Spec;
        var propsAssignment = spec.Resource.Body.First(p => p.Key == "properties");
        var propsObj = propsAssignment.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        propsObj.Properties.Should().Contain(p => p.Key == "publicNetworkAccess");
        propsObj.Properties.Should().NotContain(p => p.Key == "networkAcls");
    }

    [Fact]
    public void Given_PrivatizedStorageAccount_When_Execute_Then_InjectsNetworkAcls()
    {
        // Arrange
        var context = CreateContextWithPrivatizedResource("StorageAccount");

        // Act
        _sut.Execute(context);

        // Assert
        var spec = context.WorkItems[0].Spec;
        var propsAssignment = spec.Resource.Body.First(p => p.Key == "properties");
        var propsObj = propsAssignment.Value.Should().BeOfType<BicepObjectExpression>().Subject;

        propsObj.Properties.Should().Contain(p => p.Key == "networkAcls");
    }

    [Fact]
    public void Given_NonPrivatizedResource_When_Execute_Then_SpecUnchanged()
    {
        // Arrange
        var resource = CreateResource("kv-1", isPrivatized: false);
        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest { Resources = [resource] },
        };
        context.WorkItems.Add(CreateWorkItem(resource, "KeyVault"));
        var originalSpec = context.WorkItems[0].Spec;

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems[0].Spec.Should().BeSameAs(originalSpec);
    }

    [Fact]
    public void Given_AlreadyInjected_When_Execute_Then_NoOp()
    {
        // Arrange
        var context = CreateContextWithPrivatizedResource("KeyVault");
        _sut.Execute(context); // First injection

        var specAfterFirst = context.WorkItems[0].Spec;

        // Act — second call should be no-op
        _sut.Execute(context);

        // Assert
        context.WorkItems[0].Spec.Should().BeSameAs(specAfterFirst);
    }

    private static BicepGenerationContext CreateContextWithPrivatizedResource(string resourceTypeName)
    {
        var resource = CreateResource("res-1", isPrivatized: true);
        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest { Resources = [resource] },
        };
        context.WorkItems.Add(CreateWorkItem(resource, resourceTypeName));
        return context;
    }

    private static ResourceDefinition CreateResource(string name, bool isPrivatized)
    {
        return new ResourceDefinition
        {
            Name = name,
            Type = "Microsoft.KeyVault/vaults",
            IsPrivatized = isPrivatized,
            PrivateEndpointConfig = isPrivatized
                ? new PrivateEndpointDefinition
                {
                    VirtualNetworkId = Guid.NewGuid(),
                    SubnetName = "snet-pe",
                    DnsMode = "AutoManaged",
                }
                : null,
        };
    }

    private static ModuleWorkItem CreateWorkItem(ResourceDefinition resource, string resourceTypeName)
    {
        return new ModuleWorkItem
        {
            Resource = resource,
            Module = new GeneratedTypeModule(),
            Spec = CreateSpecWithProperties(resourceTypeName),
        };
    }

    private static BicepModuleSpec CreateSpecWithProperties(string resourceTypeName)
    {
        return new BicepModuleSpec
        {
            ModuleName = "test",
            ModuleFolderName = resourceTypeName,
            ResourceTypeName = resourceTypeName,
            Resource = new BicepResourceDeclaration
            {
                Symbol = "res",
                ArmTypeWithApiVersion = "Microsoft.Test/resources@2023-01-01",
                Body = [
                    new BicepPropertyAssignment("name", new BicepReference("name")),
                    new BicepPropertyAssignment("location", new BicepReference("location")),
                    new BicepPropertyAssignment("properties", new BicepObjectExpression([
                        new BicepPropertyAssignment("sku", new BicepStringLiteral("standard")),
                    ])),
                ],
            },
        };
    }
}
