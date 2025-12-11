using ErrorOr;
using MediatR;
using BicepGenerator.Application.Authentication.Common;

namespace BicepGenerator.Application.Authentication.Queries.Login;

public record LoginQuery(string Email, string Password) : IRequest<ErrorOr<AuthenticationResult>>;