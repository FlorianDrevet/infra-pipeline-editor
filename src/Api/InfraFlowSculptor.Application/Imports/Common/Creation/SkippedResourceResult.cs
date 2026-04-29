namespace InfraFlowSculptor.Application.Imports.Common.Creation;

/// <summary>
/// Result of a resource that could not be created, returned by <see cref="ResourceCreationCoordinator"/>.
/// </summary>
public sealed record SkippedResourceResult(string ResourceType, string Name, string Reason);
