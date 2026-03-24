namespace InfraFlowSculptor.Contracts.Common;

/// <summary>Lightweight response for a resource that depends on another resource.</summary>
/// <param name="Id">Unique identifier of the dependent resource.</param>
/// <param name="Name">Display name of the dependent resource.</param>
/// <param name="ResourceType">Type of the dependent resource (e.g. "ApplicationInsights", "WebApp").</param>
public record DependentResourceResponse(
    Guid Id,
    string Name,
    string ResourceType);
