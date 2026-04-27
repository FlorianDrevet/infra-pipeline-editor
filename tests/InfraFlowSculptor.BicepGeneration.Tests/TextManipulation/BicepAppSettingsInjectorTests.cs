using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.TextManipulation;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Tests.TextManipulation;

public sealed class BicepAppSettingsInjectorTests
{
    // ── Web/Function App module fixture ──

    private const string WebAppModule = """
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

    private const string FunctionAppModule = """
        param location string

        resource funcApp 'Microsoft.Web/sites/functionapp' = {
          name: 'myFunc'
          location: location
          properties: {
            siteConfig: {
              appSettings: [
                {
                  name: 'EXISTING'
                  value: 'val'
                }
              ]
            }
          }
        }
        """;

    private const string ContainerAppModule = """
        param location string

        resource containerApp 'Microsoft.App/containerApps' = {
          name: 'myContainer'
          location: location
          properties: {
            template: {
              containers: [
                {
                  name: 'app'
                  image: 'myimage:latest'
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

    // ── Inject routing ──

    [Fact]
    public void Given_WebAppArmType_When_Inject_Then_RoutesToWebFunctionAppSettings()
    {
        // Act
        var result = BicepAppSettingsInjector.Inject(WebAppModule, AzureResourceTypes.ArmTypes.WebApp);

        // Assert
        result.Should().Contain("param appSettings array = []");
        result.Should().NotContain("param envVars");
    }

    [Fact]
    public void Given_ContainerAppArmType_When_Inject_Then_RoutesToContainerAppEnvVars()
    {
        // Act
        var result = BicepAppSettingsInjector.Inject(ContainerAppModule, AzureResourceTypes.ArmTypes.ContainerApp);

        // Assert
        result.Should().Contain("param envVars array = []");
        result.Should().NotContain("param appSettings");
    }

    // ── InjectWebFunctionAppSettings ──

    [Fact]
    public void Given_WebAppWithSiteConfig_When_InjectWebFunctionAppSettings_Then_AddsAppSettingsToSiteConfig()
    {
        // Act
        var result = BicepAppSettingsInjector.InjectWebFunctionAppSettings(WebAppModule);

        // Assert
        result.Should().Contain("param appSettings array = []");
        result.Should().Contain("appSettings: appSettings");
    }

    [Fact]
    public void Given_FunctionAppWithExistingAppSettings_When_InjectWebFunctionAppSettings_Then_UsesConcat()
    {
        // Act
        var result = BicepAppSettingsInjector.InjectWebFunctionAppSettings(FunctionAppModule);

        // Assert
        result.Should().Contain("param appSettings array = []");
        result.Should().Contain("concat(");
        result.Should().Contain(", appSettings)");
    }

    [Fact]
    public void Given_ModuleAlreadyHasAppSettingsParam_When_InjectWebFunctionAppSettings_Then_NoOp()
    {
        // Arrange
        const string bicep = """
            param appSettings array = []
            param location string

            resource webApp 'Microsoft.Web/sites' = {
              name: 'myApp'
              location: location
              properties: {
                siteConfig: {}
              }
            }
            """;

        // Act
        var result = BicepAppSettingsInjector.InjectWebFunctionAppSettings(bicep);

        // Assert
        result.Should().Be(bicep);
    }

    // ── InjectContainerAppEnvVars ──

    [Fact]
    public void Given_ContainerAppModule_When_InjectContainerAppEnvVars_Then_AddsEnvVarsParam()
    {
        // Act
        var result = BicepAppSettingsInjector.InjectContainerAppEnvVars(ContainerAppModule);

        // Assert
        result.Should().Contain("param envVars array = []");
    }

    [Fact]
    public void Given_ModuleAlreadyHasEnvVarsParam_When_InjectContainerAppEnvVars_Then_NoOp()
    {
        // Arrange
        const string bicep = """
            param envVars array = []
            param location string

            resource containerApp 'Microsoft.App/containerApps' = {
              name: 'myContainer'
              location: location
              properties: {}
            }
            """;

        // Act
        var result = BicepAppSettingsInjector.InjectContainerAppEnvVars(bicep);

        // Assert
        result.Should().Be(bicep);
    }
}
