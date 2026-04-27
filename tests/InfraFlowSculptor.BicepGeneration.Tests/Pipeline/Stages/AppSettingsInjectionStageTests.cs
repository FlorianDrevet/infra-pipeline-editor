using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.BicepGeneration.Pipeline;
using InfraFlowSculptor.BicepGeneration.Pipeline.Stages;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Pipeline.Stages;

public sealed class AppSettingsInjectionStageTests
{
    private readonly AppSettingsInjectionStage _sut = new();

    [Fact]
    public void Given_Stage_When_CheckOrder_Then_Returns600()
    {
        _sut.Order.Should().Be(600);
    }

    [Fact]
    public void Given_WebAppWithAppSettings_When_Execute_Then_AppSettingsParamInjected()
    {
        // Arrange
        var context = CreateContext(
            "my-webapp", AzureResourceTypes.ArmTypes.WebApp,
            computeArmTypesWithAppSettings: [AzureResourceTypes.ArmTypes.WebApp]);

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems[0].Spec.Parameters.Should().Contain(p => p.Name == "appSettings");
    }

    [Fact]
    public void Given_ContainerAppWithAppSettings_When_Execute_Then_EnvVarsParamInjected()
    {
        // Arrange
        var context = CreateContext(
            "my-container", AzureResourceTypes.ArmTypes.ContainerApp,
            computeArmTypesWithAppSettings: [AzureResourceTypes.ArmTypes.ContainerApp]);

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems[0].Spec.Parameters.Should().Contain(p => p.Name == "envVars");
    }

    [Fact]
    public void Given_NonComputeResource_When_Execute_Then_NoChange()
    {
        // Arrange
        var originalSpec = CreateMinimalSpec();
        var context = CreateContext(
            "my-kv", AzureResourceTypes.ArmTypes.KeyVault,
            computeArmTypesWithAppSettings: [AzureResourceTypes.ArmTypes.WebApp],
            specOverride: originalSpec);

        // Act
        _sut.Execute(context);

        // Assert — spec should not have been mutated (no appSettings or envVars param)
        context.WorkItems[0].Spec.Parameters.Should().NotContain(p => p.Name == "appSettings");
        context.WorkItems[0].Spec.Parameters.Should().NotContain(p => p.Name == "envVars");
    }

    [Fact]
    public void Given_ComputeResourceNotInComputeArmTypesWithAppSettings_When_Execute_Then_NoChange()
    {
        // Arrange — WebApp type but no app settings target this ARM type
        var context = CreateContext(
            "my-webapp", AzureResourceTypes.ArmTypes.WebApp,
            computeArmTypesWithAppSettings: []);

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems[0].Spec.Parameters.Should().NotContain(p => p.Name == "appSettings");
    }

    // ── Helpers ──

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

    private static BicepGenerationContext CreateContext(
        string resourceName, string armType,
        HashSet<string> computeArmTypesWithAppSettings,
        BicepModuleSpec? specOverride = null)
    {
        var resource = new ResourceDefinition
        {
            ResourceId = Guid.NewGuid(),
            Name = resourceName,
            Type = armType,
        };

        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest { Resources = [resource] },
            AppSettings = new AppSettingsAnalysisResult(
                new Dictionary<string, List<(string, string, bool)>>(StringComparer.OrdinalIgnoreCase),
                computeArmTypesWithAppSettings),
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
