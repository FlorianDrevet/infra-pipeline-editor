using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.LogAnalyticsWorkspaces.Requests;

/// <summary>Request body for creating a new Log Analytics Workspace resource inside a Resource Group.</summary>
public class CreateLogAnalyticsWorkspaceRequest : LogAnalyticsWorkspaceRequestBase
{
    /// <summary>Unique identifier of the Resource Group that will own this Log Analytics Workspace.</summary>
    [Required, GuidValidation]
    public required Guid ResourceGroupId { get; init; }
}
