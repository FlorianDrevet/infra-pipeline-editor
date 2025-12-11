using BicepGenerator.Domain.UserAggregate;

namespace BicepGenerator.Application.Common.Interfaces.Authentication;

public interface IJwtGenerator
{
    string GenerateToken(User user);
}