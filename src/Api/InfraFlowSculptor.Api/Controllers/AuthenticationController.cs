using MapsterMapper;
using MediatR;
using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Application.Authentication.Commands.Register;
using InfraFlowSculptor.Application.Authentication.Queries.Login;
using InfraFlowSculptor.Contracts.Authentication;

namespace InfraFlowSculptor.Api.Controllers;

public static class AuthenticationController
{
    public static IApplicationBuilder UseAuthenticationController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var auth = endpoints.MapGroup("/auth")
                .WithTags("Authentication")
                .WithOpenApi();
            
            auth.MapPost("/register",
                    async (RegisterRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<RegisterCommand>(request);
                        var authenticationResult = await mediator.Send(command);

                        return authenticationResult.Match(
                            authenticationResult =>
                            {
                                var user = mapper.Map<AuthenticationResponse>(authenticationResult);
                                return Results.Ok(user);
                            },
                            error => error.Result());
                    })
                .WithName("Register")
                .RequireAuthorization("IsAdmin")
                .WithOpenApi();

            auth.MapPost("/login",
                    async (LoginRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var query = mapper.Map<LoginQuery>(request);
                        var authenticationResult = await mediator.Send(query);

                        return authenticationResult.Match(
                            authenticationResult =>
                            {
                                var user = mapper.Map<AuthenticationResponse>(authenticationResult);
                                return Results.Ok(user);
                            },
                            error => error.Result()
                        );
                    })
                .WithName("Login")
                .RequireRateLimiting("Login")
                .WithOpenApi();
        });
    }
}