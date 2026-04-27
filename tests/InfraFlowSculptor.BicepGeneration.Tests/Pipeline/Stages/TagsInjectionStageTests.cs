using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.BicepGeneration.Pipeline;
using InfraFlowSculptor.BicepGeneration.Pipeline.Stages;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Pipeline.Stages;

public sealed class TagsInjectionStageTests
{
    private readonly TagsInjectionStage _sut = new();

    [Fact]
    public void Given_Stage_When_CheckOrder_Then_Returns700()
    {
        _sut.Order.Should().Be(700);
    }

    [Fact]
    public void Given_WorkItem_When_Execute_Then_TagsInjectedIntoSpec()
    {
        // Arrange
        var context = CreateSingleItemContext();

        // Act
        _sut.Execute(context);

        // Assert
        var spec = context.WorkItems[0].Spec;
        spec.Parameters.Should().Contain(p => p.Name == "tags");
        spec.Resource.Body.Should().Contain(p => p.Key == "tags");
    }

    [Fact]
    public void Given_MultipleWorkItems_When_Execute_Then_AllSpecsReceiveTags()
    {
        // Arrange
        var resource1 = CreateResource("kv-1", "Microsoft.KeyVault/vaults");
        var resource2 = CreateResource("redis-1", "Microsoft.Cache/Redis");
        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest { Resources = [resource1, resource2] },
        };
        context.WorkItems.Add(new ModuleWorkItem
        {
            Resource = resource1,
            Module = new GeneratedTypeModule(),
            Spec = CreateMinimalSpec(),
        });
        context.WorkItems.Add(new ModuleWorkItem
        {
            Resource = resource2,
            Module = new GeneratedTypeModule(),
            Spec = CreateMinimalSpec(),
        });

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems.Should().AllSatisfy(item =>
            item.Spec.Parameters.Should().Contain(p => p.Name == "tags"));
    }

    [Fact]
    public void Given_SpecAlreadyHasTags_When_Execute_Then_NoOpForThatModule()
    {
        // Arrange
        var spec = CreateMinimalSpec();
        spec = spec with
        {
            Parameters = spec.Parameters.Append(new BicepParam("tags", BicepType.Object, "tags")).ToList(),
        };
        var context = CreateSingleItemContext(spec);

        // Act
        _sut.Execute(context);

        // Assert — param tags should appear exactly once
        context.WorkItems[0].Spec.Parameters.Count(p => p.Name == "tags").Should().Be(1);
    }

    // ── Helpers ──

    private static BicepGenerationContext CreateSingleItemContext(BicepModuleSpec? spec = null)
    {
        var resource = CreateResource("my-kv", "Microsoft.KeyVault/vaults");
        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest { Resources = [resource] },
        };
        context.WorkItems.Add(new ModuleWorkItem
        {
            Resource = resource,
            Module = new GeneratedTypeModule(),
            Spec = spec ?? CreateMinimalSpec(),
        });
        return context;
    }

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

    private static ResourceDefinition CreateResource(string name, string type) =>
        new() { ResourceId = Guid.NewGuid(), Name = name, Type = type };
}
