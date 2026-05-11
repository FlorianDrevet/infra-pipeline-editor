using InfraFlowSculptor.BicepGeneration.Models;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Legacy Bicep generator contract. Produces a <see cref="GeneratedTypeModule"/> from a
/// <see cref="ResourceDefinition"/>. Superseded by <see cref="IResourceTypeBicepSpecGenerator"/>
/// for the IR-based pipeline; kept alive for companion-module and parameter metadata extraction.
/// </summary>
public interface IResourceTypeBicepGenerator
{
    /// <summary>
    /// The ARM resource type identifier (e.g. <c>Microsoft.KeyVault/vaults</c>).
    /// </summary>
    string ResourceType { get; }

    /// <summary>
    /// The simple resource type name used for naming template lookups (e.g. "KeyVault").
    /// </summary>
    string ResourceTypeName { get; }

    /// <summary>
    /// Generates a legacy <see cref="GeneratedTypeModule"/> for the given <paramref name="resource"/>.
    /// </summary>
    GeneratedTypeModule Generate(ResourceDefinition resource);
}
