namespace InfraFlowSculptor.GenerationCore.Models;

/// <summary>
/// Defines the Azure DevOps Docker/ACR service connection used for a specific application environment.
/// </summary>
public sealed record ContainerRegistryServiceConnectionDefinition(
    string EnvironmentName,
    string ServiceConnectionName);