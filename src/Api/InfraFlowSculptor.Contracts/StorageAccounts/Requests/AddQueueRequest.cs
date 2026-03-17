using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.StorageAccounts.Requests;

public class AddQueueRequest
{
    [Required]
    public required string Name { get; init; }
}
