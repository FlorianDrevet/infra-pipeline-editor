using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

public class CreateInfrastructureConfigRequest
{
    [Required]
    public required string Name { get; init; }
}