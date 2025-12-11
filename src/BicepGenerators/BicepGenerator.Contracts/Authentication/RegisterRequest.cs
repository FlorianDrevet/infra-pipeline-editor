namespace BicepGenerator.Contracts.Authentication;

public record RegisterRequest(
    string Email,
    string Password,
    string Firstname,
    string Lastname);