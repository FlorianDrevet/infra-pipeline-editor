using InfraFlowSculptor.Domain.UserAggregate;

namespace InfraFlowSculptor.Application.Authentication.Common;

public record AuthenticationResult(
    User User,
    string Token);