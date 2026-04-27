using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.BicepGeneration.Pipeline;
using InfraFlowSculptor.BicepGeneration.Pipeline.Stages;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Pipeline.Stages;

public sealed class OutputInjectionStageTests
{
    private readonly OutputInjectionStage _sut = new();

    [Fact]
    public void Given_Stage_When_CheckOrder_Then_Returns500()
    {
        _sut.Order.Should().Be(500);
    }

    [Fact]
    public void Given_WorkItemWithSourceOutputs_When_Execute_Then_OutputsInjectedIntoSpec()
    {
        // Arrange
        var context = CreateContext("my-kv",
            ("my-kv", [("vaultUri", "kv.properties.vaultUri", false)]));

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems[0].Spec.Outputs.Should().Contain(o => o.Name == "vaultUri");
    }

    [Fact]
    public void Given_WorkItemNotInOutputsBySource_When_Execute_Then_SpecUnchanged()
    {
        // Arrange
        var context = CreateContext("my-redis", CreateMinimalSpec(),
            ("my-kv", [("vaultUri", "kv.properties.vaultUri", false)]));

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems[0].Spec.Outputs.Should().BeEmpty();
    }

    [Fact]
    public void Given_SecureOutput_When_Execute_Then_SecureDecoratorAdded()
    {
        // Arrange
        var context = CreateContext("my-kv",
            ("my-kv", [("secretValue", "kv.properties.secretUri", true)]));

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems[0].Spec.Outputs.Should().Contain(o => o.Name == "secretValue" && o.IsSecure);
    }

    [Fact]
    public void Given_EmptyAppSettings_When_Execute_Then_NoChanges()
    {
        // Arrange
        var resource = new ResourceDefinition
        {
            ResourceId = Guid.NewGuid(),
            Name = "my-kv",
            Type = "Microsoft.KeyVault/vaults",
        };
        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest { Resources = [resource] },
            AppSettings = AppSettingsAnalysisResult.Empty,
        };
        context.WorkItems.Add(new ModuleWorkItem
        {
            Resource = resource,
            Module = new GeneratedTypeModule(),
            Spec = CreateMinimalSpec(),
        });

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems[0].Spec.Outputs.Should().BeEmpty();
    }

    // ── Helpers ──

    private static BicepModuleSpec CreateMinimalSpec() => new()
    {
        ModuleName = "test",
        ModuleFolderName = "Test",
        ResourceTypeName = "Test",
        Resource = new BicepResourceDeclaration
        {
            Symbol = "kv",
            ArmTypeWithApiVersion = "Microsoft.KeyVault/vaults@2024-01-01",
        },
    };

    private static BicepGenerationContext CreateContext(
        string workItemResourceName,
        params (string SourceName, List<(string OutputName, string BicepExpr, bool IsSecure)> Outputs)[] outputEntries)
        => CreateContext(workItemResourceName, null, outputEntries);

    private static BicepGenerationContext CreateContext(
        string workItemResourceName,
        BicepModuleSpec? specOverride,
        params (string SourceName, List<(string OutputName, string BicepExpr, bool IsSecure)> Outputs)[] outputEntries)
    {
        var resource = new ResourceDefinition
        {
            ResourceId = Guid.NewGuid(),
            Name = workItemResourceName,
            Type = "Microsoft.KeyVault/vaults",
        };

        var outputsBySource = new Dictionary<string, List<(string, string, bool)>>(StringComparer.OrdinalIgnoreCase);
        foreach (var (sourceName, outputs) in outputEntries)
        {
            outputsBySource[sourceName] = outputs;
        }

        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest { Resources = [resource] },
            AppSettings = new AppSettingsAnalysisResult(
                outputsBySource,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase)),
        };
        context.WorkItems.Add(new ModuleWorkItem
        {
            Resource = resource,
            Module = new GeneratedTypeModule(),
            Spec = specOverride ?? CreateMinimalSpec(),
        });

        return context;
    }
}
