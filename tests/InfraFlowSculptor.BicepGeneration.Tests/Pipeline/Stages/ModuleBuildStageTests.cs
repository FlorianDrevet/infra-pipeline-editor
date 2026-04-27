using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.BicepGeneration.Pipeline;
using InfraFlowSculptor.BicepGeneration.Pipeline.Stages;
using InfraFlowSculptor.GenerationCore.Models;
using NSubstitute;

namespace InfraFlowSculptor.BicepGeneration.Tests.Pipeline.Stages;

public sealed class ModuleBuildStageTests
{
    private const string SampleBicepContent = "resource kv 'Microsoft.KeyVault/vaults' = { name: 'myKv' }";

    [Fact]
    public void Given_Stage_When_CheckOrder_Then_Returns300()
    {
        // Arrange
        var sut = new ModuleBuildStage([]);

        // Assert
        sut.Order.Should().Be(300);
    }

    [Fact]
    public void Given_ResourceWithRegisteredGenerator_When_Execute_Then_WorkItemCreated()
    {
        // Arrange
        var generator = CreateGenerator("Microsoft.KeyVault/vaults", "KeyVault");
        var sut = new ModuleBuildStage([generator]);

        var resourceId = Guid.NewGuid();
        var context = CreateContext(new[]
        {
            new ResourceDefinition
            {
                ResourceId = resourceId,
                Name = "my-keyvault",
                Type = "Microsoft.KeyVault/vaults",
                ResourceGroupName = "rg-shared",
                ResourceAbbreviation = "kv",
            },
        });

        // Act
        sut.Execute(context);

        // Assert
        context.WorkItems.Should().ContainSingle();
        var workItem = context.WorkItems[0];
        workItem.Resource.Name.Should().Be("my-keyvault");
        workItem.Module.ModuleBicepContent.Should().Be(SampleBicepContent);
        workItem.Module.ResourceGroupName.Should().Be("rg-shared");
        workItem.Module.ResourceAbbreviation.Should().Be("kv");
    }

    [Fact]
    public void Given_ResourceWithRegisteredGenerator_When_Execute_Then_ModuleNameIncludesCapitalizedIdentifier()
    {
        // Arrange
        var generator = CreateGenerator("Microsoft.KeyVault/vaults", "KeyVault", moduleName: "keyVault");
        var sut = new ModuleBuildStage([generator]);

        var context = CreateContext(new[]
        {
            new ResourceDefinition
            {
                ResourceId = Guid.NewGuid(),
                Name = "my-keyvault",
                Type = "Microsoft.KeyVault/vaults",
            },
        });

        // Act
        sut.Execute(context);

        // Assert — generator returns "keyVault", stage capitalizes the bicep identifier and appends
        var workItem = context.WorkItems[0];
        workItem.Module.ModuleName.Should().StartWith("keyVault");
    }

    [Fact]
    public void Given_ResourceWithoutRegisteredGenerator_When_Execute_Then_ThrowsNotSupportedException()
    {
        // Arrange
        var sut = new ModuleBuildStage([]);
        var context = CreateContext(new[]
        {
            new ResourceDefinition
            {
                ResourceId = Guid.NewGuid(),
                Name = "my-unknown",
                Type = "Microsoft.Unknown/resources",
            },
        });

        // Act
        var act = () => sut.Execute(context);

        // Assert
        act.Should().Throw<NotSupportedException>()
            .WithMessage("*my-unknown*Microsoft.Unknown/resources*");
    }

    [Fact]
    public void Given_MultipleResources_When_Execute_Then_ResourceIdToInfoPopulated()
    {
        // Arrange
        var generator = CreateGenerator("Microsoft.KeyVault/vaults", "KeyVault");
        var sut = new ModuleBuildStage([generator]);

        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var context = CreateContext(new[]
        {
            new ResourceDefinition { ResourceId = id1, Name = "kv-1", Type = "Microsoft.KeyVault/vaults" },
            new ResourceDefinition { ResourceId = id2, Name = "kv-2", Type = "Microsoft.KeyVault/vaults" },
        });

        // Act
        sut.Execute(context);

        // Assert
        context.ResourceIdToInfo.Should().ContainKey(id1);
        context.ResourceIdToInfo[id1].Name.Should().Be("kv-1");
        context.ResourceIdToInfo[id1].ResourceTypeName.Should().Be("KeyVault");
        context.ResourceIdToInfo.Should().ContainKey(id2);
    }

    [Fact]
    public void Given_ResourceWithEmptyGuid_When_Execute_Then_NotAddedToResourceIdToInfo()
    {
        // Arrange
        var generator = CreateGenerator("Microsoft.KeyVault/vaults", "KeyVault");
        var sut = new ModuleBuildStage([generator]);

        var context = CreateContext(new[]
        {
            new ResourceDefinition { ResourceId = Guid.Empty, Name = "kv-1", Type = "Microsoft.KeyVault/vaults" },
        });

        // Act
        sut.Execute(context);

        // Assert
        context.ResourceIdToInfo.Should().BeEmpty();
        context.WorkItems.Should().ContainSingle(); // work item still created
    }

    // ── Helpers ──

    private static IResourceTypeBicepGenerator CreateGenerator(
        string armType, string typeName, string moduleName = "module")
    {
        var generator = Substitute.For<IResourceTypeBicepGenerator>();
        generator.ResourceType.Returns(armType);
        generator.ResourceTypeName.Returns(typeName);
        generator.Generate(Arg.Any<ResourceDefinition>()).Returns(new GeneratedTypeModule
        {
            ModuleName = moduleName,
            ModuleBicepContent = SampleBicepContent,
            ModuleFileName = "kv.module.bicep",
            ModuleFolderName = typeName,
            ResourceTypeName = typeName,
        });
        return generator;
    }

    private static BicepGenerationContext CreateContext(ResourceDefinition[] resources) =>
        new()
        {
            Request = new GenerationRequest
            {
                Resources = resources,
            },
        };
}
