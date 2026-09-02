namespace InfraFlowSculptor.Contracts.Catalogs;

/// <summary>
/// Response DTO for a single test framework definition in the catalog.
/// </summary>
public sealed record TestFrameworkDefinitionResponse
{
    /// <summary>Application stack identifier (e.g. "DotNet", "NodeJs", "Angular").</summary>
    public required string Stack { get; init; }

    /// <summary>Framework identifier in PascalCase (e.g. "XUnit", "Jest", "Vitest").</summary>
    public required string FrameworkKey { get; init; }

    /// <summary>Human-readable display name (e.g. "xUnit", "Jest").</summary>
    public required string DisplayName { get; init; }

    /// <summary>Default test command for pipeline generation.</summary>
    public required string DefaultCommand { get; init; }

    /// <summary>Default test results format (e.g. "VSTest", "JUnit").</summary>
    public required string DefaultResultsFormat { get; init; }

    /// <summary>Default coverage tool, or null if not applicable.</summary>
    public string? DefaultCoverageTool { get; init; }

    /// <summary>Default glob pattern for coverage reports.</summary>
    public required string DefaultCoverageReportGlob { get; init; }
}
