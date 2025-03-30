using ErrorOr;
using MediatR;
using InfraFlowSculptor.Application.Authentication.Common;

namespace InfraFlowSculptor.Application.Authentication.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string Firstname,
    string Lastname) : IRequest<ErrorOr<AuthenticationResult>>;