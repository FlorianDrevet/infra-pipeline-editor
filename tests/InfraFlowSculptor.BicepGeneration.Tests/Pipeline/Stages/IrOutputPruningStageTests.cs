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
        // Arrange — only "vaultUri" is registered as used for the kv module
        var context = CreateContextWithSingleModule(
            modulePath: "modules/KeyVault/kv.module.bicep",
            symbolName: "kv",
            outputs: ["vaultUri", "id", "name"],
            usedOutputsByModulePath: new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["modules/KeyVault/kv.module.bicep"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "vaultUri" },
            });

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
        // Arrange — "VAULTURI" registered with different case than the spec output "vaultUri"
        var context = CreateContextWithSingleModule(
            modulePath: "modules/KeyVault/kv.module.bicep",
            symbolName: "kv",
            outputs: ["vaultUri"],
            usedOutputsByModulePath: new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["modules/KeyVault/kv.module.bicep"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "VAULTURI" },
            });

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
            usedOutputsByModulePath: new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase));

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
        // Arrange — two modules, each used map keeps a single output
        var usedMap = new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["modules/KeyVault/kv.module.bicep"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "vaultUri" },
            ["modules/RedisCache/redis.module.bicep"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "hostName" },
        };

        var context = CreateEmptyContext(usedMap);
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
        // Arrange — used map is empty, so nothing references this module's outputs
        var context = CreateContextWithSingleModule(
            modulePath: "modules/KeyVault/kv.module.bicep",
            symbolName: "kv",
            outputs: ["vaultUri", "id"],
            usedOutputsByModulePath: new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase));

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems[0].Spec.Outputs.Should().BeEmpty();
    }

    [Fact]
    public void Execute_SkipsWorkItemsWithoutSpec()
    {
        // Arrange — legacy work item with a Spec that has no outputs and an empty used map.
        var context = CreateContextWithSingleModule(
            modulePath: "modules/Legacy/legacy.module.bicep",
            symbolName: "legacy",
            outputs: [],
            usedOutputsByModulePath: new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase));

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
            usedOutputsByModulePath: new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["modules/KeyVault/kv.module.bicep"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "vaultUri" },
            });

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

    private static BicepGenerationContext CreateContextWithSingleModule(
        string modulePath,
        string symbolName,
        IReadOnlyList<string> outputs,
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> usedOutputsByModulePath)
    {
        var context = CreateEmptyContext(usedOutputsByModulePath);
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

    private static BicepGenerationContext CreateEmptyContext(
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> usedOutputsByModulePath)
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
                MainBicep = string.Empty,
                ModuleFiles = new Dictionary<string, string>(),
                UsedOutputsByModulePath = usedOutputsByModulePath,
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
