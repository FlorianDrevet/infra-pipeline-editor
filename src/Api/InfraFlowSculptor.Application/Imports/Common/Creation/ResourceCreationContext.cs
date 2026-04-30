using InfraFlowSculptor.Application.Imports.Common.Properties;

namespace InfraFlowSculptor.Application.Imports.Common.Creation;

/// <summary>
/// Groups dependency resolution context for resource creation commands.
/// </summary>
/// <param name="CreatedResourcesByType">Resources already created indexed by Azure resource type identifier.</param>
/// <param name="TypedProperties">Strongly-typed properties extracted from the import source for the current resource.</param>
/// <param name="CreatedResourcesByName">Resources already created indexed by source name (used when dependencies are matched by name).</param>
/// <param name="DependencyResourceNames">Source names of dependencies declared by the current resource.</param>
public sealed record ResourceCreationContext(
    IReadOnlyDictionary<string, Guid> CreatedResourcesByType,
    IExtractedResourceProperties? TypedProperties = null,
    IReadOnlyDictionary<string, Guid>? CreatedResourcesByName = null,
    IReadOnlyList<string>? DependencyResourceNames = null);
