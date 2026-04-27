using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.BicepGeneration.Pipeline;
using InfraFlowSculptor.BicepGeneration.Pipeline.Stages;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Pipeline.Stages;

/// <summary>
/// Unit tests for <see cref="IrOutputPruningStage"/>. The stage prunes unused outputs
/// from each module's <see cref="BicepModuleSpec"/> based on references discovered in
/// <c>main.bicep</c>, then re-emits the pruned module Bicep text into the result's
/// <see cref="GenerationResult.ModuleFiles"/>.
/// </summary>
public sealed class IrOutputPruningStageTests
{
    private readonly IrOutputPruningStage _sut = new();

    [Fact]
    public void Order_Equals_950()
    {
        _sut.Order.Should().Be(950);
    }

    [Fact]
    public void Execute_RemovesOutputs_WhenNotInUsedMap()
    {
        // Arrange — main.bicep references only "vaultUri" on kvModule
        var context = CreateContextWithSingleModule(
            modulePath: "modules/KeyVault/kv.module.bicep",
            symbolName: "kv",
            outputs: ["vaultUri", "id", "name"],
            mainBicep: BuildMainBicep("kv", "modules/KeyVault/kv.module.bicep", "vaultUri"));

        // Act
        _sut.Execute(context);

        // Assert
        var workItem = context.WorkItems[0];
        workItem.Spec.Outputs.Should().HaveCount(1);
        workItem.Spec.Outputs.Should().ContainSingle(o => o.Name == "vaultUri");

        var moduleContent = context.Result!.ModuleFiles["modules/KeyVault/kv.module.bicep"];
        moduleContent.Should().Contain("output vaultUri");
        moduleContent.Should().NotContain("output id ");
        moduleContent.Should().NotContain("output name ");
    }

    [Fact]
    public void Execute_KeepsOutputs_WhenInUsedMap_CaseInsensitive()
    {
        // Arrange — main.bicep references "VAULTURI" (different case) on kvModule
        var context = CreateContextWithSingleModule(
            modulePath: "modules/KeyVault/kv.module.bicep",
            symbolName: "kv",
            outputs: ["vaultUri"],
            mainBicep: BuildMainBicep("kv", "modules/KeyVault/kv.module.bicep", "VAULTURI"));

        // Act
        _sut.Execute(context);

        // Assert — case-insensitive match keeps the output
        context.WorkItems[0].Spec.Outputs.Should().ContainSingle(o => o.Name == "vaultUri");
    }

    [Fact]
    public void Execute_IsNoOp_WhenSpecHasEmptyOutputs()
    {
        // Arrange
        var context = CreateContextWithSingleModule(
            modulePath: "modules/Test/test.module.bicep",
            symbolName: "test",
            outputs: [],
            mainBicep: "// no module references");

        var originalContent = context.Result!.ModuleFiles["modules/Test/test.module.bicep"];

        // Act
        _sut.Execute(context);

        // Assert — content unchanged
        context.WorkItems[0].Spec.Outputs.Should().BeEmpty();
        context.Result.ModuleFiles["modules/Test/test.module.bicep"].Should().Be(originalContent);
    }

    [Fact]
    public void Execute_PrunesEachModuleIndependently()
    {
        // Arrange — two modules, main.bicep uses only one output from each
        var mainBicep = $$"""
            module kvModule './modules/KeyVault/kv.module.bicep' = {}
            module redisModule './modules/RedisCache/redis.module.bicep' = {}

            var a = kvModule.outputs.vaultUri
            var b = redisModule.outputs.hostName
            """;

        var context = CreateEmptyContext(mainBicep);
        AddWorkItem(context, "modules/KeyVault/kv.module.bicep", "kv", "KeyVault", ["vaultUri", "id"]);
        AddWorkItem(context, "modules/RedisCache/redis.module.bicep", "redis", "RedisCache", ["hostName", "port"]);

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems[0].Spec.Outputs.Should().ContainSingle(o => o.Name == "vaultUri");
        context.WorkItems[1].Spec.Outputs.Should().ContainSingle(o => o.Name == "hostName");
    }

    [Fact]
    public void Execute_RemovesAllOutputs_WhenModulePathMissingFromUsedMap()
    {
        // Arrange — main.bicep does not reference this module at all
        var context = CreateContextWithSingleModule(
            modulePath: "modules/KeyVault/kv.module.bicep",
            symbolName: "kv",
            outputs: ["vaultUri", "id"],
            mainBicep: "// main.bicep with no module references");

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems[0].Spec.Outputs.Should().BeEmpty();
    }

    [Fact]
    public void Execute_SkipsWorkItemsWithoutSpec()
    {
        // Arrange — legacy work item with a Spec that has no outputs and a main.bicep
        // that doesn't reference it — stage must run without throwing.
        var context = CreateContextWithSingleModule(
            modulePath: "modules/Legacy/legacy.module.bicep",
            symbolName: "legacy",
            outputs: [],
            mainBicep: "// empty");

        // Act
        var act = () => _sut.Execute(context);

        // Assert — should not throw
        act.Should().NotThrow();
    }

    [Fact]
    public void Execute_DoesNothing_WhenContextSkipOutputPruningIsTrue()
    {
        // Arrange
        var context = CreateContextWithSingleModule(
            modulePath: "modules/KeyVault/kv.module.bicep",
            symbolName: "kv",
            outputs: ["vaultUri", "id"],
            mainBicep: BuildMainBicep("kv", "modules/KeyVault/kv.module.bicep", "vaultUri"));

        context.SkipOutputPruning = true;
        var originalSpecOutputCount = context.WorkItems[0].Spec.Outputs.Count;
        var originalContent = context.Result!.ModuleFiles["modules/KeyVault/kv.module.bicep"];

        // Act
        _sut.Execute(context);

        // Assert — neither Spec nor module file were touched
        context.WorkItems[0].Spec.Outputs.Should().HaveCount(originalSpecOutputCount);
        context.Result.ModuleFiles["modules/KeyVault/kv.module.bicep"].Should().Be(originalContent);
    }

    // ── Helpers ──

    private static string BuildMainBicep(string symbolName, string modulePath, string outputName)
    {
        return $$"""
            module {{symbolName}}Module './{{modulePath}}' = {}

            var x = {{symbolName}}Module.outputs.{{outputName}}
            """;
    }

    private static BicepGenerationContext CreateContextWithSingleModule(
        string modulePath,
        string symbolName,
        IReadOnlyList<string> outputs,
        string mainBicep)
    {
        var context = CreateEmptyContext(mainBicep);
        var folderAndFile = SplitModulePath(modulePath);
        AddWorkItem(context, modulePath, symbolName, folderAndFile.Folder, outputs);
        return context;
    }

    private static (string Folder, string FileName) SplitModulePath(string modulePath)
    {
        // modulePath = "modules/<folder>/<file>"
        var withoutPrefix = modulePath["modules/".Length..];
        var firstSlash = withoutPrefix.IndexOf('/');
        return (withoutPrefix[..firstSlash], withoutPrefix[(firstSlash + 1)..]);
    }

    private static BicepGenerationContext CreateEmptyContext(string mainBicep)
    {
        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest
            {
                Resources = [],
                ResourceGroups = [],
                Environments = [],
                EnvironmentNames = [],
                RoleAssignments = [],
                AppSettings = [],
                ExistingResourceReferences = [],
                NamingContext = new NamingContext(),
            },
            Result = new GenerationResult
            {
                MainBicep = mainBicep,
                ModuleFiles = new Dictionary<string, string>(),
            },
        };
        return context;
    }

    private static void AddWorkItem(
        BicepGenerationContext context,
        string modulePath,
        string symbolName,
        string folderName,
        IReadOnlyList<string> outputs)
    {
        var (folder, fileName) = SplitModulePath(modulePath);

        var spec = new BicepModuleSpec
        {
            ModuleName = symbolName,
            ModuleFolderName = folder,
            ModuleFileName = fileName,
            ResourceTypeName = folder,
            Resource = new BicepResourceDeclaration
            {
                Symbol = symbolName,
                ArmTypeWithApiVersion = $"Microsoft.Test/{folder}@2024-01-01",
            },
            Outputs = outputs.Select(o =>
                new BicepOutput(o, BicepType.String, new BicepRawExpression($"{symbolName}.properties.{o}")))
                .ToList(),
        };

        var resource = new ResourceDefinition
        {
            ResourceId = Guid.NewGuid(),
            Name = symbolName,
            Type = $"Microsoft.Test/{folder}",
        };

        var workItem = new ModuleWorkItem
        {
            Resource = resource,
            Module = new GeneratedTypeModule
            {
                ModuleName = symbolName,
                ModuleFileName = fileName,
                ModuleFolderName = folder,
                ResourceTypeName = folder,
            },
            Spec = spec,
        };

        context.WorkItems.Add(workItem);

        // Also register an existing module file in the result so the stage has something to rewrite.
        var existingFiles = (Dictionary<string, string>)context.Result!.ModuleFiles;
        var initialContent = "// initial content with " + string.Join(", ", outputs.Select(o => $"output {o} ..."));
        existingFiles[modulePath] = initialContent;
    }
}
