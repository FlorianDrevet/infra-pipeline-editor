using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Ir;

namespace InfraFlowSculptor.BicepGeneration.Pipeline.Stages;

/// <summary>
/// Stage 300 — Initial module construction.
/// </summary>
/// <remarks>
/// <para><b>Pre-conditions:</b> <see cref="BicepGenerationContext.Request"/> populated.</para>
/// <para><b>Post-conditions:</b> <see cref="BicepGenerationContext.WorkItems"/> contains one
/// <see cref="ModuleWorkItem"/> per resource (with the per-resource Bicep template produced by the
/// matching <see cref="IResourceTypeBicepGenerator"/>) and <see cref="BicepGenerationContext.ResourceIdToInfo"/>
/// is populated for parent-reference resolution.</para>
/// <para>Throws <see cref="NotSupportedException"/> when a resource has no registered generator.</para>
/// </remarks>
public sealed class ModuleBuildStage : IBicepGenerationStage
{
    private readonly IReadOnlyDictionary<string, IResourceTypeBicepGenerator> _generatorByArmType;

    /// <summary>
    /// Creates the stage with the available <see cref="IResourceTypeBicepGenerator"/> implementations.
    /// </summary>
    public ModuleBuildStage(IEnumerable<IResourceTypeBicepGenerator> generators)
    {
        _generatorByArmType = generators.ToDictionary(g => g.ResourceType, g => g, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public int Order => 300;

    /// <inheritdoc />
    public void Execute(BicepGenerationContext context)
    {
        var request = context.Request;

        context.ResourceIdToInfo = request.Resources
            .Where(r => r.ResourceId != Guid.Empty)
            .ToDictionary(r => r.ResourceId, r =>
            {
                var typeName = _generatorByArmType.TryGetValue(r.Type, out var gen) ? gen.ResourceTypeName : string.Empty;
                return (Name: r.Name, ResourceTypeName: typeName);
            });

        foreach (var resource in request.Resources)
        {
            if (!_generatorByArmType.TryGetValue(resource.Type, out var generator))
            {
                throw new NotSupportedException(
                    $"No Bicep generator registered for resource '{resource.Name}' with resource type '{resource.Type}'.");
            }

            // Try IR path first for generators migrated to Builder + IR.
            BicepModuleSpec? spec = null;
            var module = generator.Generate(resource);

            if (generator is IResourceTypeBicepSpecGenerator specGen)
            {
                spec = specGen.GenerateSpec(resource);
                // Preserve Parameters (user-configured values) and CompanionModules from legacy Generate()
                // — the skeleton only carries structural metadata.
                module = LegacyTextModuleAdapter.CreateSkeletonModule(spec) with
                {
                    Parameters = module.Parameters,
                    CompanionModules = module.CompanionModules,
                    ParameterGroupMappings = module.ParameterGroupMappings,
                };
            }

            var resourceIdentifier = BicepIdentifierHelper.ToBicepIdentifier(resource.Name);
            var moduleName = $"{module.ModuleName}{Capitalize(resourceIdentifier)}";

            // Apply per-resource fields here (the bare module returned by the generator carries no
            // resource-specific naming context). Identity, outputs, app settings, tags and parent
            // references are populated by subsequent stages.
            context.WorkItems.Add(new ModuleWorkItem
            {
                Resource = resource,
                Spec = spec,
                Module = module with
                {
                    ModuleName = moduleName,
                    ResourceGroupName = resource.ResourceGroupName,
                    LogicalResourceName = resource.Name,
                    ResourceAbbreviation = resource.ResourceAbbreviation,
                },
            });
        }
    }

    private static string Capitalize(string value) =>
        value.Length == 0 ? value : char.ToUpperInvariant(value[0]) + value[1..];
}
