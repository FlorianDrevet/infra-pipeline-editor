using InfraFlowSculptor.Domain.UserAggregate;

namespace InfraFlowSculptor.Application.Common.Interfaces.Authentication;

public interface IJwtGenerator
{
    string GenerateToken(User user);
}