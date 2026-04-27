using InfraFlowSculptor.BicepGeneration.Ir.Emit;
using InfraFlowSculptor.BicepGeneration.Models;

namespace InfraFlowSculptor.BicepGeneration.Ir;

/// <summary>
/// Bridges between the IR (<see cref="BicepModuleSpec"/>) and the legacy
/// <see cref="GeneratedTypeModule"/> DTO consumed by the assembler.
/// </summary>
internal static class LegacyTextModuleAdapter
{
    /// <summary>
    /// Creates a skeleton <see cref="GeneratedTypeModule"/> from a <see cref="BicepModuleSpec"/>
    /// with assembly-relevant metadata. Module content is left empty (populated by
    /// <see cref="Pipeline.Stages.SpecEmissionStage"/> after all transformations).
    /// </summary>
    internal static GeneratedTypeModule CreateSkeletonModule(BicepModuleSpec spec)
    {
        return new GeneratedTypeModule
        {
            ModuleName = spec.ModuleName,
            ModuleFileName = spec.ModuleFileName ?? spec.ModuleName,
            ModuleFolderName = spec.ModuleFolderName,
            ResourceTypeName = spec.ResourceTypeName,
            SecureParameters = spec.Parameters
                .Where(p => p.IsSecure)
                .Select(p => p.Name)
                .ToList(),
            ParameterTypeOverrides = spec.Parameters
                .Where(p => p.Type is BicepCustomType)
                .ToDictionary(p => p.Name, p => ((BicepCustomType)p.Type).Name),
            Parameters = new Dictionary<string, object>(),
        };
    }

    /// <summary>
    /// Emits the module and types content from a <see cref="BicepModuleSpec"/> and applies
    /// them to the given <see cref="GeneratedTypeModule"/>.
    /// </summary>
    internal static GeneratedTypeModule EmitContent(
        GeneratedTypeModule module,
        BicepModuleSpec spec,
        BicepEmitter emitter)
    {
        return module with
        {
            ModuleBicepContent = emitter.EmitModule(spec),
            ModuleTypesBicepContent = emitter.EmitTypes(spec),
        };
    }
}
