using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.Projects.Common;

namespace InfraFlowSculptor.Application.Projects.Queries.ValidateRecentItems;

/// <summary>Query to validate a list of recently viewed items against current user access.</summary>
/// <param name="Items">The list of item references (id + type) to validate.</param>
public record ValidateRecentItemsQuery(
    IReadOnlyList<RecentItemReference> Items) : IQuery<List<RecentItemResult>>;

/// <summary>A single recently viewed item reference to validate.</summary>
/// <param name="Id">The unique identifier of the item.</param>
/// <param name="Type">The type of item: "project" or "config".</param>
public record RecentItemReference(Guid Id, string Type);
