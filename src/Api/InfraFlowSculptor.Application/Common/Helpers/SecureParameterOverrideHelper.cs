using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.Application.Common.Helpers;

/// <summary>
/// Helper for deriving secure parameter overrides, integrating custom variable group mappings
/// when configured by the user.
/// </summary>
public static class SecureParameterOverrideHelper
{
    /// <summary>
    /// Derives secure parameter overrides and integrates custom variable group mappings.
    /// Returns the list of auto-derived overrides (for params without custom mappings)
    /// and merges custom-mapped params into the provided variable group definitions.
    /// </summary>
    /// <param name="resources">The resource definitions from the generation context.</param>
    /// <param name="generators">The registered Bicep generators.</param>
    /// <param name="secureParameterMappings">Custom mappings configured by the user.</param>
    /// <param name="variableGroups">Mutable list of variable group definitions to merge custom mappings into.</param>
    /// <returns>The list of auto-derived secure parameter override names (not custom-mapped).</returns>
    public static List<string> DeriveSecureParameterOverrides(
        IEnumerable<ResourceDefinition> resources,
        IEnumerable<IResourceTypeBicepGenerator> generators,
        IReadOnlyList<SecureParameterMappingReadModel>? secureParameterMappings,
        List<PipelineVariableGroupDefinition> variableGroups)
    {
        var autoDerived = new List<string>();
        var mappingLookup = (secureParameterMappings ?? [])
            .Where(m => m.VariableGroupId.HasValue && m.PipelineVariableName is not null)
            .ToDictionary(m => (m.ResourceId, m.SecureParameterName));

        foreach (var resource in resources)
        {
            var generator = generators.FirstOrDefault(g => g.ResourceType == resource.Type);
            if (generator is null)
                continue;

            var module = generator.Generate(resource);
            var resourceIdentifier = ToBicepIdentifier(resource.Name);
            var qualifiedModuleName = $"{module.ModuleName}{Capitalize(resourceIdentifier)}";

            foreach (var secureParam in module.SecureParameters)
            {
                var fullBicepParamName = $"{qualifiedModuleName}{Capitalize(secureParam)}";

                // Check if the user configured a custom mapping for this param
                // Resource ID lookup: we need to match by resource name since ResourceDefinition doesn't carry the GUID
                var customMapping = secureParameterMappings?
                    .FirstOrDefault(m =>
                        m.ResourceName == resource.Name &&
                        m.SecureParameterName == secureParam &&
                        m.VariableGroupId.HasValue &&
                        m.PipelineVariableName is not null);

                if (customMapping is not null)
                {
                    // Merge into the variable group definitions
                    var vgName = customMapping.VariableGroupName!;
                    var existingVg = variableGroups.FirstOrDefault(
                        g => string.Equals(g.GroupName, vgName, StringComparison.OrdinalIgnoreCase));

                    var mapping = new PipelineVariableMappingDefinition
                    {
                        PipelineVariableName = customMapping.PipelineVariableName!,
                        BicepParameterName = fullBicepParamName,
                    };

                    if (existingVg is not null)
                    {
                        // Add to existing group's mappings
                        var currentMappings = existingVg.Mappings.ToList();
                        currentMappings.Add(mapping);
                        existingVg.Mappings = currentMappings;
                    }
                    else
                    {
                        // Create a new variable group definition
                        variableGroups.Add(new PipelineVariableGroupDefinition
                        {
                            GroupName = vgName,
                            Mappings = [mapping],
                        });
                    }
                }
                else
                {
                    // No custom mapping → auto-derived override
                    autoDerived.Add(fullBicepParamName);
                }
            }
        }

        return autoDerived;
    }

    /// <summary>
    /// Replicates <c>BicepIdentifierHelper.ToBicepIdentifier</c> logic (camelCase from hyphens/underscores/spaces).
    /// </summary>
    private static string ToBicepIdentifier(string name)
    {
        if (string.IsNullOrEmpty(name)) return "resource";
        var parts = name.Split(['-', '_', ' '], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "resource";
        var sb = new System.Text.StringBuilder(parts[0].ToLowerInvariant());
        foreach (var part in parts.Skip(1))
        {
            if (part.Length > 0)
                sb.Append(char.ToUpperInvariant(part[0])).Append(part[1..].ToLowerInvariant());
        }
        return sb.ToString();
    }

    private static string Capitalize(string s) =>
        s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];
}
