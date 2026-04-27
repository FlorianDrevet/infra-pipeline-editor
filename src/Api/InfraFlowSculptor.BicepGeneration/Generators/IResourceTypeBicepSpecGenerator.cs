using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Extended generator interface for generators migrated to the Builder + IR pattern (Vague 2).
/// Generators implementing this interface produce a structured <see cref="BicepModuleSpec"/>
/// instead of a raw string template. The pipeline uses IR transformers and emits the final
/// text via <see cref="Ir.Emit.BicepEmitter"/>.
/// </summary>
public interface IResourceTypeBicepSpecGenerator : IResourceTypeBicepGenerator
{
    /// <summary>
    /// Generates a structured IR specification for this resource type.
    /// The spec is transformed by pipeline stages and emitted to text by <see cref="Ir.Emit.BicepEmitter"/>.
    /// </summary>
    BicepModuleSpec GenerateSpec(ResourceDefinition resource);
}
