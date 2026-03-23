using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.RedisCaches.Requests;

/// <summary>Request body for creating a new Redis Cache resource inside a Resource Group.</summary>
public class CreateRedisCacheRequest : RedisCacheRequestBase
{
    /// <summary>Unique identifier of the Resource Group that will own this Redis Cache.</summary>
    [Required, GuidValidation]
    public required Guid ResourceGroupId { get; init; }
}
