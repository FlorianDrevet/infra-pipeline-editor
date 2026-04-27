using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.BicepGeneration.Pipeline;
using InfraFlowSculptor.BicepGeneration.Pipeline.Stages;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Pipeline.Stages;

public sealed class AppSettingsInjectionStageTests
{
    private readonly AppSettingsInjectionStage _sut = new();

    private const string WebAppModuleBicep = """
        param location string

        resource webApp 'Microsoft.Web/sites' = {
          name: 'myApp'
          location: location
          properties: {
            siteConfig: {
              linuxFxVersion: 'DOTNET|8.0'
            }
          }
        }
        """;

    private const string ContainerAppModuleBicep = """
        param location string

        resource containerApp 'Microsoft.App/containerApps' = {
          name: 'myContainer'
          location: location
          properties: {
            template: {
              containers: [
                {
                  name: 'app'
                  image: 'img:latest'
                  resources: {
                    cpu: '0.5'
                    memory: '1Gi'
                  }
                }
              ]
            }
          }
        }
        """;

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
            "my-webapp", AzureResourceTypes.ArmTypes.WebApp, WebAppModuleBicep,
            computeArmTypesWithAppSettings: [AzureResourceTypes.ArmTypes.WebApp]);

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems[0].Module.ModuleBicepContent.Should().Contain("param appSettings array");
    }

    [Fact]
    public void Given_ContainerAppWithAppSettings_When_Execute_Then_EnvVarsParamInjected()
    {
        // Arrange
        var context = CreateContext(
            "my-container", AzureResourceTypes.ArmTypes.ContainerApp, ContainerAppModuleBicep,
            computeArmTypesWithAppSettings: [AzureResourceTypes.ArmTypes.ContainerApp]);

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems[0].Module.ModuleBicepContent.Should().Contain("param envVars array");
    }

    [Fact]
    public void Given_NonComputeResource_When_Execute_Then_NoChange()
    {
        // Arrange
        const string kvModule = "param location string\nresource kv 'Microsoft.KeyVault/vaults' = {}";
        var context = CreateContext(
            "my-kv", AzureResourceTypes.ArmTypes.KeyVault, kvModule,
            computeArmTypesWithAppSettings: [AzureResourceTypes.ArmTypes.WebApp]);

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems[0].Module.ModuleBicepContent.Should().Be(kvModule);
    }

    [Fact]
    public void Given_ComputeResourceNotInComputeArmTypesWithAppSettings_When_Execute_Then_NoChange()
    {
        // Arrange — WebApp type but no app settings target this ARM type
        var context = CreateContext(
            "my-webapp", AzureResourceTypes.ArmTypes.WebApp, WebAppModuleBicep,
            computeArmTypesWithAppSettings: []);

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems[0].Module.ModuleBicepContent.Should().Be(WebAppModuleBicep);
    }

    // ── Helpers ──

    private static BicepGenerationContext CreateContext(
        string resourceName, string armType, string moduleBicep,
        HashSet<string> computeArmTypesWithAppSettings)
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
            Module = new GeneratedTypeModule { ModuleBicepContent = moduleBicep },
        });

        return context;
    }
}
