namespace InfraFlowSculptor.Mcp.Common;

/// <summary>
/// Metadata record for a supported repository topology.
/// </summary>
/// <param name="Id">Topology identifier matching <see cref="Domain.ProjectAggregate.ValueObjects.LayoutPresetEnum"/>.</param>
/// <param name="Label">Human-readable label.</param>
/// <param name="Description">Detailed description of the topology.</param>
/// <param name="RequiredRepositoryCount">Number of repositories required at project level.</param>
/// <param name="RepositoryContentKinds">Content kind combinations per repository slot.</param>
internal sealed record TopologyInfo(
    string Id,
    string Label,
    string Description,
    int RequiredRepositoryCount,
    string[][] RepositoryContentKinds);
