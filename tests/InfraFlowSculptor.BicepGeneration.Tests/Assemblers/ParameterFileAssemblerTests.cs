using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Assemblers;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;
using System.Text.Json.Serialization;

namespace InfraFlowSculptor.BicepGeneration.Tests.Assemblers;

public sealed class ParameterFileAssemblerTests
{
    [Fact]
    public void Given_TypedParameterObjectWithJsonPropertyNames_When_EnvironmentOverridesApply_Then_BicepParamUsesAnnotatedNames()
    {
        // Arrange
        var modules = new[]
        {
            new GeneratedTypeModule
            {
                ModuleName = "containerAppMyApp",
                Parameters = new Dictionary<string, object>
                {
                    ["containerRuntime"] = new TestContainerRuntimeParameters
                    {
                        Image = "mcr.microsoft.com/app:latest",
                        CpuCores = "0.25",
                        MemoryGi = "0.5Gi",
                    },
                },
                ParameterGroupMappings = new Dictionary<string, (string GroupKey, string PropertyName)>
                {
                    ["cpuCores"] = ("containerRuntime", "cpuCores"),
                },
            },
        };

        var environments = new[]
        {
            new EnvironmentDefinition
            {
                Name = "dev",
                ShortName = "dev",
            },
        };

        var resources = new[]
        {
            new ResourceDefinition
            {
                Name = "my-app",
                Type = AzureResourceTypes.ArmTypes.ContainerAppType,
                EnvironmentConfigs = new Dictionary<string, IReadOnlyDictionary<string, string>>
                {
                    ["dev"] = new Dictionary<string, string>
                    {
                        ["cpuCores"] = "1",
                    },
                },
            },
        };

        // Act
        var result = ParameterFileAssembler.GenerateEnvironmentParameterFiles(modules, environments, resources, []);

        // Assert
        result["main.dev.bicepparam"].Should().Contain("param containerAppMyAppContainerRuntime = {");
        result["main.dev.bicepparam"].Should().Contain("image: 'mcr.microsoft.com/app:latest'");
        result["main.dev.bicepparam"].Should().Contain("cpuCores: '1'");
        result["main.dev.bicepparam"].Should().Contain("memoryGi: '0.5Gi'");
        result["main.dev.bicepparam"].Should().NotContain("Image:");
        result["main.dev.bicepparam"].Should().NotContain("CpuCores:");
        result["main.dev.bicepparam"].Should().NotContain("MemoryGi:");
    }

    [Fact]
    public void Given_DictionaryParameterGroup_When_EnvironmentOverridesApply_Then_BicepParamUsesMergedDictionaryValues()
    {
        // Arrange
        var modules = new[]
        {
            new GeneratedTypeModule
            {
                ModuleName = "containerAppMyApp",
                Parameters = new Dictionary<string, object>
                {
                    ["containerRuntime"] = new Dictionary<string, object>
                    {
                        ["cpuCores"] = "0.25",
                        ["memoryGi"] = "0.5Gi",
                    },
                },
                ParameterGroupMappings = new Dictionary<string, (string GroupKey, string PropertyName)>
                {
                    ["cpuCores"] = ("containerRuntime", "cpuCores"),
                },
            },
        };

        var environments = new[]
        {
            new EnvironmentDefinition
            {
                Name = "dev",
                ShortName = "dev",
            },
        };

        var resources = new[]
        {
            new ResourceDefinition
            {
                Name = "my-app",
                Type = AzureResourceTypes.ArmTypes.ContainerAppType,
                EnvironmentConfigs = new Dictionary<string, IReadOnlyDictionary<string, string>>
                {
                    ["dev"] = new Dictionary<string, string>
                    {
                        ["cpuCores"] = "1",
                    },
                },
            },
        };

        // Act
        var result = ParameterFileAssembler.GenerateEnvironmentParameterFiles(modules, environments, resources, []);

        // Assert
        result["main.dev.bicepparam"].Should().Contain("param containerAppMyAppContainerRuntime = {");
        result["main.dev.bicepparam"].Should().Contain("cpuCores: '1'");
        result["main.dev.bicepparam"].Should().Contain("memoryGi: '0.5Gi'");
    }

    [Fact]
    public void Given_EmptyModulesBetweenEmittedModules_When_GeneratingParameterFiles_Then_DoesNotAccumulateBlankLines()
    {
        // Arrange
        var modules = new[]
        {
            new GeneratedTypeModule
            {
                ModuleName = "sqlDatabaseIfs",
                Parameters = new Dictionary<string, object>
                {
                    ["sku"] = "Basic",
                },
            },
            new GeneratedTypeModule
            {
                ModuleName = "containerRegistryIfs",
                Parameters = new Dictionary<string, object>(),
            },
            new GeneratedTypeModule
            {
                ModuleName = "managedIdentityIfsApi",
                Parameters = new Dictionary<string, object>(),
            },
            new GeneratedTypeModule
            {
                ModuleName = "containerAppIfsApi",
                Parameters = new Dictionary<string, object>
                {
                    ["containerImage"] = "ifs/backend",
                },
            },
        };

        var environments = new[]
        {
            new EnvironmentDefinition
            {
                Name = "dev",
                ShortName = "dev",
            },
        };

        // Act
        var result = ParameterFileAssembler.GenerateEnvironmentParameterFiles(modules, environments, [], []);
        var parameterFile = result["main.dev.bicepparam"].ReplaceLineEndings("\n");

        // Assert
        parameterFile.Should().Contain("param sqlDatabaseIfsSku = 'Basic'");
        parameterFile.Should().Contain("param containerAppIfsApiContainerImage = 'ifs/backend'");
        parameterFile.Should().NotContain("\n\n\n");
    }

    [Fact]
    public void Given_NestedParameterObject_When_GeneratingParameterFiles_Then_NestedBicepObjectRemainsIndented()
    {
        // Arrange
        var modules = new[]
        {
            new GeneratedTypeModule
            {
                ModuleName = "containerAppMyApp",
                Parameters = new Dictionary<string, object>
                {
                    ["healthProbes"] = new TestHealthProbesParameters
                    {
                        Readiness = new TestProbeParameters
                        {
                            Path = "/healthz/ready",
                            Port = 8080,
                        },
                    },
                },
            },
        };

        var environments = new[]
        {
            new EnvironmentDefinition
            {
                Name = "dev",
                ShortName = "dev",
            },
        };

        var resources = new[]
        {
            new ResourceDefinition
            {
                Name = "my-app",
                Type = AzureResourceTypes.ArmTypes.ContainerAppType,
                EnvironmentConfigs = new Dictionary<string, IReadOnlyDictionary<string, string>>(),
            },
        };

        var expectedSnippet = "param containerAppMyAppHealthProbes = {\n  readiness: {\n    path: '/healthz/ready'\n    port: 8080\n  }\n}";

        // Act
        var result = ParameterFileAssembler.GenerateEnvironmentParameterFiles(modules, environments, resources, []);
        var parameterFile = result["main.dev.bicepparam"].ReplaceLineEndings("\n");

        // Assert
        parameterFile.Should().Contain(expectedSnippet);
    }

    [Fact]
    public void Given_VirtualNetworkPerEnvironmentOverrides_When_GeneratingParameterFiles_Then_EmitsArrayAddressPrefixesAndDdosBoolPerEnvironment()
    {
        // Arrange
        var modules = new[]
        {
            new GeneratedTypeModule
            {
                ModuleName = "virtualNetworkMyVnet",
                Parameters = new Dictionary<string, object>
                {
                    ["addressPrefixes"] = new List<object> { "10.0.0.0/16" },
                    ["enableDdosProtection"] = false,
                },
            },
        };

        var environments = new[]
        {
            new EnvironmentDefinition { Name = "dev", ShortName = "dev" },
            new EnvironmentDefinition { Name = "prod", ShortName = "prod" },
        };

        var resources = new[]
        {
            new ResourceDefinition
            {
                Name = "my-vnet",
                Type = AzureResourceTypes.ArmTypes.VirtualNetworkType,
                EnvironmentConfigs = new Dictionary<string, IReadOnlyDictionary<string, string>>
                {
                    ["dev"] = new Dictionary<string, string>
                    {
                        ["addressPrefixes"] = "[\"10.1.0.0/16\"]",
                        ["enableDdosProtection"] = "false",
                    },
                    ["prod"] = new Dictionary<string, string>
                    {
                        ["addressPrefixes"] = "[\"10.2.0.0/16\",\"10.3.0.0/16\"]",
                        ["enableDdosProtection"] = "true",
                    },
                },
            },
        };

        // Act
        var result = ParameterFileAssembler.GenerateEnvironmentParameterFiles(modules, environments, resources, []);
        var dev = result["main.dev.bicepparam"].ReplaceLineEndings("\n");
        var prod = result["main.prod.bicepparam"].ReplaceLineEndings("\n");

        // Assert — address spaces emitted as a real Bicep array (opener right after '='),
        // not a quoted JSON string, and DDoS as a bool, distinct per environment.
        dev.Should().Contain("param virtualNetworkMyVnetAddressPrefixes = [");
        dev.Should().Contain("'10.1.0.0/16'");
        dev.Should().NotContain("'[\"10.1.0.0/16\"]'");
        dev.Should().Contain("param virtualNetworkMyVnetEnableDdosProtection = false");

        prod.Should().Contain("param virtualNetworkMyVnetAddressPrefixes = [");
        prod.Should().Contain("'10.2.0.0/16'");
        prod.Should().Contain("'10.3.0.0/16'");
        prod.Should().Contain("param virtualNetworkMyVnetEnableDdosProtection = true");
    }

    private sealed class TestContainerRuntimeParameters
    {
        [JsonPropertyName("image")]
        public string Image { get; init; } = string.Empty;

        [JsonPropertyName("cpuCores")]
        public string CpuCores { get; init; } = string.Empty;

        [JsonPropertyName("memoryGi")]
        public string MemoryGi { get; init; } = string.Empty;
    }

    private sealed class TestHealthProbesParameters
    {
        [JsonPropertyName("readiness")]
        public TestProbeParameters? Readiness { get; init; }
    }

    private sealed class TestProbeParameters
    {
        [JsonPropertyName("path")]
        public string Path { get; init; } = string.Empty;

        [JsonPropertyName("port")]
        public int Port { get; init; }
    }
}
