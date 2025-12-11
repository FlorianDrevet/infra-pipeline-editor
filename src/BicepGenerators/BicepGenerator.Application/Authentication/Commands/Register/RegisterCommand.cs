using ErrorOr;
using MediatR;
using BicepGenerator.Application.Authentication.Common;

namespace BicepGenerator.Application.Authentication.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string Firstname,
    string Lastname) : IRequest<ErrorOr<AuthenticationResult>>;