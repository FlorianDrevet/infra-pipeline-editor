namespace InfraFlowSculptor.Infrastructure.Persistence.Views;

/// <summary>
/// Keyless entity mapped to the <c>vw_ChildToParentLinks</c> PostgreSQL view.
/// Aggregates child-to-parent FK relationships across the 5 parent-child resource types
/// (WebApp→AppServicePlan, FunctionApp→AppServicePlan, ContainerApp→ContainerAppEnvironment,
/// SqlDatabase→SqlServer, ApplicationInsights→LogAnalyticsWorkspace) into a single queryable surface.
/// </summary>
public sealed class ChildToParentLinkView
{
    /// <summary>Gets the child resource identifier.</summary>
    public Guid ChildResourceId { get; init; }

    /// <summary>Gets the parent resource identifier.</summary>
    public Guid ParentResourceId { get; init; }

    /// <summary>Gets the resource group identifier of the child resource.</summary>
    public Guid ResourceGroupId { get; init; }
}
