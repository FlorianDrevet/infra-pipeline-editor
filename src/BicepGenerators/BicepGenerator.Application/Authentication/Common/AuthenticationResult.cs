using BicepGenerator.Domain.UserAggregate;

namespace BicepGenerator.Application.Authentication.Common;

public record AuthenticationResult(
    User User,
    string Token);