namespace InfraFlowSculptor.BicepGeneration.Ir;

/// <summary>
/// An existing resource reference (e.g. <c>resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' existing = { name: sqlServerName }</c>).
/// Used to declare parent resources or cross-module references.
/// </summary>
public sealed record BicepExistingResource(
    string Symbol,
    string ArmTypeWithApiVersion,
    string NameExpression);
