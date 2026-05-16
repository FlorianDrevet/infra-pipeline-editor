using System.Text.Json.Serialization;

namespace InfraFlowSculptor.BicepGeneration.Generators.ParameterModels.ContainerApp;

internal sealed record ContainerRuntimeParameters
{
    [JsonPropertyName("cpuCores")]
    public required string CpuCores { get; init; }

    [JsonPropertyName("memoryGi")]
    public required string MemoryGi { get; init; }
}