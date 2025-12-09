using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Authentication.Requests;

public class LoginRequest {
    [Required]
    public required string Email { get; init; }
    
    [Required]
    public required string Password { get; init; }
}