using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Authentication.Requests;

public class RegisterRequest
{
    [Required]
    public required string Email { get; init; }
    
    [Required]
    public required string Password { get; init; }
    
    [Required]
    public required string Firstname { get; init; }
    
    [Required]
    public required string Lastname { get; init; }
}