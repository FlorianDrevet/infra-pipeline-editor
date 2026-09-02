using InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

namespace InfraFlowSculptor.Domain.Common.Catalogs.TestFrameworks;

/// <summary>
/// Strongly-typed identifier for a test framework, scoped to an application stack.
/// </summary>
/// <param name="Stack">The application stack this framework belongs to.</param>
/// <param name="Framework">The framework identifier in PascalCase (e.g. "XUnit", "Jest").</param>
public sealed record TestFrameworkKey(
    ApplicationStack.ApplicationStackEnum Stack,
    string Framework);
