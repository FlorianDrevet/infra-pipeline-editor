namespace InfraFlowSculptor.Application.Imports.Common.Creation;

/// <summary>
/// Result of a successfully created resource, returned by <see cref="ResourceCreationCoordinator"/>.
/// </summary>
public sealed record CreatedResourceResult(string ResourceType, string ResourceId, string Name);
