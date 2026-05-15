using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.BicepGeneration.Pipeline;
using InfraFlowSculptor.BicepGeneration.Pipeline.Stages;
using InfraFlowSculptor.GenerationCore.Models;
using NSubstitute;

namespace InfraFlowSculptor.BicepGeneration.Tests.Pipeline.Stages;

public sealed class ModuleBuildStageTests
{
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
        workItem.Module.ResourceGroupName.Should().Be("rg-shared");
        workItem.Module.ResourceAbbreviation.Should().Be("kv");
        workItem.Spec.Should().NotBeNull();
        workItem.Spec.ModuleName.Should().Be("keyVault");
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

    [Fact]
    public void Given_CancelledTokenBeforeExecution_When_Execute_Then_ThrowsOperationCanceledException()
    {
        // Arrange
        var generator = CreateGenerator("Microsoft.KeyVault/vaults", "KeyVault");
        var sut = new ModuleBuildStage([generator]);
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        var context = CreateContext(
            [
                new ResourceDefinition
                {
                    ResourceId = Guid.NewGuid(),
                    Name = "my-keyvault",
                    Type = "Microsoft.KeyVault/vaults",
                },
            ],
            cancellationTokenSource.Token);

        // Act
        var act = () => sut.Execute(context);

        // Assert
        act.Should().Throw<OperationCanceledException>();
        generator.DidNotReceive().GenerateSpec(Arg.Any<ResourceDefinition>());
        generator.DidNotReceive().Generate(Arg.Any<ResourceDefinition>());
    }

    [Fact]
    public void Given_TokenCancelledAfterSpecGeneration_When_Execute_Then_DoesNotInvokeLegacyGenerator()
    {
        // Arrange
        const string armType = "Microsoft.KeyVault/vaults";
        const string typeName = "KeyVault";

        using var cancellationTokenSource = new CancellationTokenSource();
        var spec = CreateSpec(armType, typeName);
        var generator = Substitute.For<IResourceTypeBicepSpecGenerator>();
        generator.ResourceType.Returns(armType);
        generator.ResourceTypeName.Returns(typeName);
        generator.GenerateSpec(Arg.Any<ResourceDefinition>()).Returns(_ =>
        {
            cancellationTokenSource.Cancel();
            return spec;
        });
        generator.Generate(Arg.Any<ResourceDefinition>()).Returns(CreateLegacyModule(typeName));

        var sut = new ModuleBuildStage([generator]);
        var context = CreateContext(
            [
                new ResourceDefinition
                {
                    ResourceId = Guid.NewGuid(),
                    Name = "my-keyvault",
                    Type = armType,
                },
            ],
            cancellationTokenSource.Token);

        // Act
        var act = () => sut.Execute(context);

        // Assert
        act.Should().Throw<OperationCanceledException>();
        generator.Received(1).GenerateSpec(Arg.Any<ResourceDefinition>());
        generator.DidNotReceive().Generate(Arg.Any<ResourceDefinition>());
        context.WorkItems.Should().BeEmpty();
    }

    [Fact]
    public void Given_TokenCancelledBetweenResources_When_Execute_Then_StopsBeforeNextResource()
    {
        // Arrange
        const string armType = "Microsoft.KeyVault/vaults";
        const string typeName = "KeyVault";

        using var cancellationTokenSource = new CancellationTokenSource();
        var generator = Substitute.For<IResourceTypeBicepSpecGenerator>();
        generator.ResourceType.Returns(armType);
        generator.ResourceTypeName.Returns(typeName);
        generator.GenerateSpec(Arg.Any<ResourceDefinition>()).Returns(CreateSpec(armType, typeName));

        var generatedResourceCount = 0;
        generator.Generate(Arg.Any<ResourceDefinition>()).Returns(_ =>
        {
            generatedResourceCount++;
            if (generatedResourceCount == 1)
            {
                cancellationTokenSource.Cancel();
            }

            return CreateLegacyModule(typeName);
        });

        var sut = new ModuleBuildStage([generator]);
        var context = CreateContext(
            [
                new ResourceDefinition
                {
                    ResourceId = Guid.NewGuid(),
                    Name = "my-keyvault-01",
                    Type = armType,
                },
                new ResourceDefinition
                {
                    ResourceId = Guid.NewGuid(),
                    Name = "my-keyvault-02",
                    Type = armType,
                },
            ],
            cancellationTokenSource.Token);

        // Act
        var act = () => sut.Execute(context);

        // Assert
        act.Should().Throw<OperationCanceledException>();
        generator.Received(1).GenerateSpec(Arg.Any<ResourceDefinition>());
        generator.Received(1).Generate(Arg.Any<ResourceDefinition>());
        context.WorkItems.Should().ContainSingle();
    }

    // ── Helpers ──

    private static BicepModuleSpec CreateSpec(string armType, string typeName, string moduleName = "keyVault") =>
        new()
        {
            ModuleName = moduleName,
            ModuleFolderName = typeName,
            ResourceTypeName = typeName,
            Resource = new BicepResourceDeclaration
            {
                Symbol = "kv",
                ArmTypeWithApiVersion = $"{armType}@2024-01-01",
            },
        };

    private static GeneratedTypeModule CreateLegacyModule(string typeName, string moduleName = "keyVault") =>
        new()
        {
            ModuleName = moduleName,
            ModuleFileName = "kv.module.bicep",
            ModuleFolderName = typeName,
            ResourceTypeName = typeName,
        };

    private static IResourceTypeBicepSpecGenerator CreateGenerator(
        string armType, string typeName, string moduleName = "keyVault")
    {
        var generator = Substitute.For<IResourceTypeBicepSpecGenerator>();
        generator.ResourceType.Returns(armType);
        generator.ResourceTypeName.Returns(typeName);
        generator.GenerateSpec(Arg.Any<ResourceDefinition>()).Returns(CreateSpec(armType, typeName, moduleName));
        generator.Generate(Arg.Any<ResourceDefinition>()).Returns(CreateLegacyModule(typeName, moduleName));
        return generator;
    }

    private static BicepGenerationContext CreateContext(
        ResourceDefinition[] resources,
        CancellationToken cancellationToken = default) =>
        new()
        {
            Request = new GenerationRequest
            {
                Resources = resources,
            },
            CancellationToken = cancellationToken,
        };
}
