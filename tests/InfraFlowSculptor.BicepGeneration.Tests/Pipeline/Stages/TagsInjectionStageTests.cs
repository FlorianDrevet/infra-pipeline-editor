using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.BicepGeneration.Pipeline;
using InfraFlowSculptor.BicepGeneration.Pipeline.Stages;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Pipeline.Stages;

public sealed class TagsInjectionStageTests
{
    private readonly TagsInjectionStage _sut = new();

    private const string MinimalModuleBicep = """
        @description('Location for all resources')
        param location string

        resource kv 'Microsoft.KeyVault/vaults' = {
          name: 'myKv'
          location: location
          properties: {}
        }
        """;

    [Fact]
    public void Given_Stage_When_CheckOrder_Then_Returns700()
    {
        _sut.Order.Should().Be(700);
    }

    [Fact]
    public void Given_WorkItem_When_Execute_Then_TagsInjectedIntoModule()
    {
        // Arrange
        var context = CreateSingleItemContext(MinimalModuleBicep);

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems[0].Module.ModuleBicepContent.Should().Contain("param tags object");
        context.WorkItems[0].Module.ModuleBicepContent.Should().Contain("tags: tags");
    }

    [Fact]
    public void Given_MultipleWorkItems_When_Execute_Then_AllModulesReceiveTags()
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
            Module = new GeneratedTypeModule { ModuleBicepContent = MinimalModuleBicep },
        });
        context.WorkItems.Add(new ModuleWorkItem
        {
            Resource = resource2,
            Module = new GeneratedTypeModule { ModuleBicepContent = MinimalModuleBicep },
        });

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems.Should().AllSatisfy(item =>
            item.Module.ModuleBicepContent.Should().Contain("param tags object"));
    }

    [Fact]
    public void Given_ModuleAlreadyHasTags_When_Execute_Then_NoOpForThatModule()
    {
        // Arrange
        const string moduleWithTags = """
            param location string
            param tags object = {}

            resource kv 'Microsoft.KeyVault/vaults' = {
              name: 'myKv'
              location: location
              tags: tags
              properties: {}
            }
            """;
        var context = CreateSingleItemContext(moduleWithTags);

        // Act
        _sut.Execute(context);

        // Assert — param tags should appear exactly once
        var paramCount = context.WorkItems[0].Module.ModuleBicepContent
            .Split("param tags").Length - 1;
        paramCount.Should().Be(1);
    }

    // ── Helpers ──

    private static BicepGenerationContext CreateSingleItemContext(string moduleBicep)
    {
        var resource = CreateResource("my-kv", "Microsoft.KeyVault/vaults");
        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest { Resources = [resource] },
        };
        context.WorkItems.Add(new ModuleWorkItem
        {
            Resource = resource,
            Module = new GeneratedTypeModule { ModuleBicepContent = moduleBicep },
        });
        return context;
    }

    private static ResourceDefinition CreateResource(string name, string type) =>
        new() { ResourceId = Guid.NewGuid(), Name = name, Type = type };
}
