using InfraFlowSculptor.Api.Controllers.Constants;
using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Application.Git.Commands.VerifyGitConnection;
using InfraFlowSculptor.Contracts.Git.Requests;
using InfraFlowSculptor.Contracts.Git.Responses;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>Minimal API endpoint definitions for stateless Git operations.</summary>
public static class GitController
{
    /// <summary>Registers the Git endpoints on the application builder.</summary>
    public static IApplicationBuilder UseGitController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup(Routes.Git)
                .WithTags("Git");

            group.MapPost("/verify-connection",
                    async ([FromBody] VerifyGitConnectionRequest request,
                        IMediator mediator,
                        IMapper mapper) =>
                    {
                        var command = mapper.Map<VerifyGitConnectionCommand>(request);
                        var result = await mediator.Send(command);
                        return result.Match(
                            value => TypedResults.Ok(mapper.Map<VerifyGitConnectionResponse>(value)),
                            errors => errors.Result());
                    })
                .WithName(GitRouteNames.VerifyConnection)
                .WithSummary("Verify a Git repository connection")
                .WithDescription("Stateless verification of a Git repository connection. Returns available branches and a default branch candidate.")
                .Produces<VerifyGitConnectionResponse>(StatusCodes.Status200OK)
                .ProducesValidationProblem()
                .ProducesProblem(StatusCodes.Status401Unauthorized);
        });
    }
}
