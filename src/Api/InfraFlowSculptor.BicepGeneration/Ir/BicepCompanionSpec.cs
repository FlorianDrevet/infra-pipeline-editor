namespace InfraFlowSculptor.BicepGeneration.Ir;

/// <summary>
/// Specification for a companion module deployed alongside a primary resource
/// (e.g. blob and table services for a Storage Account).
/// </summary>
public sealed record BicepCompanionSpec(
    string ModuleName,
    string FolderName,
    BicepModuleSpec Spec);
