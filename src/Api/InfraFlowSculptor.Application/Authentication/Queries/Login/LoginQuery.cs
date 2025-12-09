using ErrorOr;
using MediatR;
using InfraFlowSculptor.Application.Authentication.Common;

namespace InfraFlowSculptor.Application.Authentication.Queries.Login;

public record LoginQuery(string Email, string Password) : IRequest<ErrorOr<AuthenticationResult>>;