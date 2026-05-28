namespace InfraFlowSculptor.Domain.Common.Catalogs.TestFrameworks;

/// <summary>
/// Immutable definition of a test framework within the catalog.
/// Each definition describes the framework's defaults for pipeline generation.
/// </summary>
/// <param name="Key">Strongly-typed composite key (Stack + Framework).</param>
/// <param name="DisplayName">Human-readable display name.</param>
/// <param name="DefaultCommand">Default test command for pipeline generation.</param>
/// <param name="DefaultResultsFormat">Default test results format (e.g. "VSTest", "JUnit").</param>
/// <param name="DefaultCoverageTool">Default coverage tool (e.g. "Cobertura", "JaCoCo"), or null if none.</param>
/// <param name="DefaultCoverageReportGlob">Default glob pattern for coverage reports.</param>
public sealed record TestFrameworkDefinition(
    TestFrameworkKey Key,
    string DisplayName,
    string DefaultCommand,
    string DefaultResultsFormat,
    string? DefaultCoverageTool,
    string DefaultCoverageReportGlob);
