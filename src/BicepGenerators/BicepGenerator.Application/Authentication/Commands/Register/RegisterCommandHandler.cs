using ErrorOr;
using MediatR;
using BicepGenerator.Application.Authentication.Common;
using BicepGenerator.Application.Common.Interfaces.Authentication;
using BicepGenerator.Application.Common.Interfaces.Persistence;
using BicepGenerator.Domain.Common.Errors;
using BicepGenerator.Domain.UserAggregate;
using BicepGenerator.Domain.UserAggregate.ValueObjects;

namespace BicepGenerator.Application.Authentication.Commands.Register;

public class RegisterCommandHandler(
    IJwtGenerator jwtGenerator,
    IHashPassword hashPassword,
    IUserRepository userRepository) : IRequestHandler<RegisterCommand, ErrorOr<AuthenticationResult>>
{
    public async Task<ErrorOr<AuthenticationResult>> Handle(RegisterCommand command,
        CancellationToken cancellationToken)
    {
        if (await userRepository.GetUserByEmailAsync(command.Email) is not null)
        {
            return Errors.User.DuplicateEmailError();
        }

        var hashedPassword = hashPassword.GetHashedPassword(command.Password);
        var user = User
            .Create(command.Email, hashedPassword.Item2, new Name(command.Firstname, command.Lastname),
                hashedPassword.Item1);
        await userRepository.AddAsync(user);

        var token = jwtGenerator.GenerateToken(user);
        return new AuthenticationResult(user, token);
    }
}