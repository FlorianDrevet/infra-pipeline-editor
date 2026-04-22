using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.CustomDomains.Requests;

/// <summary>Request to add a custom domain binding to a compute resource.</summary>
public class AddCustomDomainRequest
{
    /// <summary>The deployment environment name (e.g. "production", "staging").</summary>
    [Required]
    [MaxLength(100)]
    public required string EnvironmentName { get; init; }

    /// <summary>The fully qualified domain name (e.g. "api.example.com").</summary>
    [Required]
    [MaxLength(253)]
    public required string DomainName { get; init; }

    /// <summary>SSL binding type: "SniEnabled" (default) or "Disabled".</summary>
    [MaxLength(20)]
    public string BindingType { get; init; } = "SniEnabled";
}
