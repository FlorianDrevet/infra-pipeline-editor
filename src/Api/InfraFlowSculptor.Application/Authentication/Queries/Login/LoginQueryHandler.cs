using ErrorOr;
using MediatR;
using InfraFlowSculptor.Application.Authentication.Common;
using InfraFlowSculptor.Application.Common.Interfaces.Authentication;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.UserAggregate;

namespace InfraFlowSculptor.Application.Authentication.Queries.Login;

public class LoginQueryHandler(IJwtGenerator jwtGenerator, IUserRepository userRepository, IHashPassword hashPassword) :
    IRequestHandler<LoginQuery, ErrorOr<AuthenticationResult>>
{
    public async Task<ErrorOr<AuthenticationResult>> Handle(LoginQuery query, CancellationToken cancellationToken)
    {
        if (await userRepository.GetUserByEmailAsync(query.Email) is not User user)
        {
            return Errors.Authentication.InvalidUsername();
        }

        var hashedPassword = hashPassword.GetHashedPassword(query.Password, user.Salt);
        if (user.Password != hashedPassword)
        {
            return Errors.Authentication.InvalidPassword();
        }

        var token = jwtGenerator.GenerateToken(user);

        return new AuthenticationResult(user, token);
    }
}