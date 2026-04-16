using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.Common.Requests;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>Request body for updating an existing project environment definition.</summary>
public class UpdateProjectEnvironmentRequest
{
    /// <summary>New display name for the environment.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Short environment identifier without separators (e.g. "dev", "qa", "prod").</summary>
    [Required]
    public required string ShortName { get; init; }

    /// <summary>Short prefix prepended to generated resource names.</summary>
    public string Prefix { get; init; } = string.Empty;

    /// <summary>Short suffix appended to generated resource names.</summary>
    public string Suffix { get; init; } = string.Empty;

    /// <summary>Azure region where resources in this environment will be deployed.</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    /// <summary>Azure subscription ID where resources in this environment will be created.</summary>
    [Required, GuidValidation]
    public required Guid SubscriptionId { get; init; }

    /// <summary>Deployment ordering index. Lower values are deployed first.</summary>
    public int Order { get; init; } = 0;

    /// <summary>When true, deployments targeting this environment require explicit approval before proceeding.</summary>
    public bool RequiresApproval { get; init; } = false;

    /// <summary>Azure DevOps service connection name used for ARM deployments in this environment.</summary>
    public string? AzureResourceManagerConnection { get; init; }

    /// <summary>Full replacement list of Azure tags for this environment.</summary>
    public IReadOnlyCollection<TagRequest> Tags { get; init; } = [];
}
