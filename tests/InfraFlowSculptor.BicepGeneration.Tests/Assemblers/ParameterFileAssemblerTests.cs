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
                Type = AzureResourceTypes.ArmTypes.ContainerApp,
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

    private sealed class TestContainerRuntimeParameters
    {
        [JsonPropertyName("image")]
        public string Image { get; init; } = string.Empty;

        [JsonPropertyName("cpuCores")]
        public string CpuCores { get; init; } = string.Empty;

        [JsonPropertyName("memoryGi")]
        public string MemoryGi { get; init; } = string.Empty;
    }
}