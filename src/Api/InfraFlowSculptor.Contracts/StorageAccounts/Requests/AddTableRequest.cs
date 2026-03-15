using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.StorageAccounts.Requests;

public class AddTableRequest
{
    [Required]
    public required string Name { get; init; }
}
