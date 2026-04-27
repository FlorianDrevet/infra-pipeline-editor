using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.BicepGeneration.Pipeline;
using InfraFlowSculptor.BicepGeneration.Pipeline.Stages;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Pipeline.Stages;

public sealed class OutputInjectionStageTests
{
    private readonly OutputInjectionStage _sut = new();

    private const string ModuleWithResource = """
        param location string

        resource kv 'Microsoft.KeyVault/vaults' = {
          name: 'myKv'
          location: location
          properties: {}
        }
        """;

    [Fact]
    public void Given_Stage_When_CheckOrder_Then_Returns500()
    {
        _sut.Order.Should().Be(500);
    }

    [Fact]
    public void Given_WorkItemWithSourceOutputs_When_Execute_Then_OutputsInjectedIntoModule()
    {
        // Arrange
        var context = CreateContext("my-kv", ModuleWithResource,
            ("my-kv", [("vaultUri", "kv.properties.vaultUri", false)]));

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems[0].Module.ModuleBicepContent
            .Should().Contain("output vaultUri string = kv.properties.vaultUri");
    }

    [Fact]
    public void Given_WorkItemNotInOutputsBySource_When_Execute_Then_ModuleUnchanged()
    {
        // Arrange
        var context = CreateContext("my-redis", ModuleWithResource,
            ("my-kv", [("vaultUri", "kv.properties.vaultUri", false)]));

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems[0].Module.ModuleBicepContent.Trim()
            .Should().Be(ModuleWithResource.Trim());
    }

    [Fact]
    public void Given_SecureOutput_When_Execute_Then_SecureDecoratorAdded()
    {
        // Arrange
        var context = CreateContext("my-kv", ModuleWithResource,
            ("my-kv", [("secretValue", "kv.properties.secretUri", true)]));

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems[0].Module.ModuleBicepContent.Should().Contain("@secure()");
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
            Module = new GeneratedTypeModule { ModuleBicepContent = ModuleWithResource },
        });

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems[0].Module.ModuleBicepContent.Trim()
            .Should().Be(ModuleWithResource.Trim());
    }

    // ── Helpers ──

    private static BicepGenerationContext CreateContext(
        string workItemResourceName,
        string moduleBicep,
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
            Module = new GeneratedTypeModule { ModuleBicepContent = moduleBicep },
        });

        return context;
    }
}
