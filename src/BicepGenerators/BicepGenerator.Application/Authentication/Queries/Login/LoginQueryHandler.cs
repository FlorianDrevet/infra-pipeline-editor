using ErrorOr;
using MediatR;
using BicepGenerator.Application.Authentication.Common;
using BicepGenerator.Application.Common.Interfaces.Authentication;
using BicepGenerator.Application.Common.Interfaces.Persistence;
using BicepGenerator.Domain.Common.Errors;
using BicepGenerator.Domain.UserAggregate;

namespace BicepGenerator.Application.Authentication.Queries.Login;

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