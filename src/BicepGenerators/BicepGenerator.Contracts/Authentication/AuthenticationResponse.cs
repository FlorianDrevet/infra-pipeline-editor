namespace BicepGenerator.Contracts.Authentication;

public record AuthenticationResponse(
    Guid Id,
    string Email,
    string Token);