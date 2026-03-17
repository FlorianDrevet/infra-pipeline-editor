using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

public class UpdateEnvironmentRequest
{
    [Required]
    public required string Name { get; init; }

    public string Prefix { get; init; } = string.Empty;

    public string Suffix { get; init; } = string.Empty;

    [Required]
    public required string Location { get; init; }

    [Required, GuidValidation]
    public required Guid TenantId { get; init; }

    [Required, GuidValidation]
    public required Guid SubscriptionId { get; init; }

    public int Order { get; init; } = 0;

    public bool RequiresApproval { get; init; } = false;

    public IReadOnlyList<TagRequest> Tags { get; init; } = [];
}
