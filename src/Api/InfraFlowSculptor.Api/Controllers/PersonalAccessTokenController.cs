using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Application.PersonalAccessTokens.Commands.CreatePersonalAccessToken;
using InfraFlowSculptor.Application.PersonalAccessTokens.Commands.RevokePersonalAccessToken;
using InfraFlowSculptor.Application.PersonalAccessTokens.Queries.ListPersonalAccessTokens;
using InfraFlowSculptor.Contracts.PersonalAccessTokens.Requests;
using InfraFlowSculptor.Contracts.PersonalAccessTokens.Responses;
using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate.ValueObjects;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>
/// Minimal API endpoints for personal access token (PAT) management.
/// </summary>
public static class PersonalAccessTokenController
{
    /// <summary>
    /// Maps the personal access token endpoint group to the application pipeline.
    /// </summary>
    public static IApplicationBuilder UsePersonalAccessTokenController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/personal-access-tokens")
                .WithTags("Personal Access Tokens");

            group.MapGet("",
                    async (IMediator mediator, IMapper mapper) =>
                    {
                        var query = new ListPersonalAccessTokensQuery();
                        var result = await mediator.Send(query);

                        return result.Match(
                            tokens =>
                            {
                                var response = tokens
                                    .Select(t => mapper.Map<PersonalAccessTokenResponse>(t))
                                    .ToList();
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("ListPersonalAccessTokens")
                .WithSummary("List personal access tokens")
                .WithDescription("Returns all personal access tokens belonging to the current authenticated user. Token values are not included — only metadata.")
                .Produces<List<PersonalAccessTokenResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized);

            group.MapPost("",
                    async (CreatePersonalAccessTokenRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<CreatePersonalAccessTokenCommand>(request);
                        var result = await mediator.Send(command);

                        return result.Match(
                            created =>
                            {
                                var tokenResponse = mapper.Map<PersonalAccessTokenResponse>(created.Token);
                                var response = new CreatedPersonalAccessTokenResponse(tokenResponse, created.PlainTextToken);
                                return TypedResults.Created(
                                    uri: (string?)null,
                                    value: response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("CreatePersonalAccessToken")
                .WithSummary("Create a personal access token")
                .WithDescription("Generates a new personal access token for the current user. The plaintext token is returned only once in the response — store it securely.")
                .Produces<CreatedPersonalAccessTokenResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized);

            group.MapDelete("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator) =>
                    {
                        var command = new RevokePersonalAccessTokenCommand(new PersonalAccessTokenId(id));
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("RevokePersonalAccessToken")
                .WithSummary("Revoke a personal access token")
                .WithDescription("Revokes the specified personal access token. Once revoked, the token can no longer be used for authentication.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status409Conflict);
        });
    }
}
