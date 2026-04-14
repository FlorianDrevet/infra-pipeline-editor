using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.Common.Requests;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>Request body for adding a new environment definition to a project.</summary>
public class AddProjectEnvironmentRequest
{
    /// <summary>Unique display name for the environment (e.g. "Production", "Staging").</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Short environment identifier without separators (e.g. "dev", "qa", "prod").</summary>
    [Required]
    public required string ShortName { get; init; }

    /// <summary>Optional short prefix prepended to generated resource names (e.g. "prod").</summary>
    public string Prefix { get; init; } = string.Empty;

    /// <summary>Optional short suffix appended to generated resource names (e.g. "001").</summary>
    public string Suffix { get; init; } = string.Empty;

    /// <summary>Azure region where resources in this environment will be deployed (e.g. "westeurope").</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    /// <summary>Azure subscription ID where resources in this environment will be created.</summary>
    [Required, GuidValidation]
    public required Guid SubscriptionId { get; init; }

    /// <summary>Deployment ordering index. Lower values are deployed first. Defaults to 0.</summary>
    public int Order { get; init; } = 0;

    /// <summary>When true, deployments targeting this environment require explicit approval before proceeding.</summary>
    public bool RequiresApproval { get; init; } = false;

    /// <summary>Azure DevOps service connection name used for ARM deployments in this environment (e.g. "mySubscriptionDev").</summary>
    public string? AzureResourceManagerConnection { get; init; }

    /// <summary>Optional list of Azure tags to apply to all resources deployed in this environment.</summary>
    public IReadOnlyList<TagRequest> Tags { get; init; } = [];
}
