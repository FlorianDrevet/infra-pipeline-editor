namespace InfraFlowSculptor.Infrastructure.Persistence.Views;

/// <summary>
/// Keyless entity mapped to the <c>vw_ResourceEnvironmentEntries</c> PostgreSQL view.
/// Aggregates environment entries across all 17 typed environment settings tables
/// into a single queryable surface.
/// </summary>
public sealed class ResourceEnvironmentEntryView
{
    /// <summary>Gets the resource group identifier (used for filtering).</summary>
    public Guid ResourceGroupId { get; init; }

    /// <summary>Gets the Azure resource identifier.</summary>
    public Guid ResourceId { get; init; }

    /// <summary>Gets the environment name (e.g. "dev", "prod").</summary>
    public string EnvironmentName { get; init; } = string.Empty;
}
