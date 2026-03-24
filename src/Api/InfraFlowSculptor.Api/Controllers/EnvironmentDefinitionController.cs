using InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddEnvironment;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveEnvironment;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.UpdateEnvironment;
using MediatR;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

public static class EnvironmentDefinitionController
{
    public static IApplicationBuilder UseEnvironmentDefinitionController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var environments = endpoints.MapGroup("/infra-config/{id:guid}/environments")
                .WithTags("Environment Definitions");

            environments.MapPost("",
                    async ([FromRoute] Guid id, AddEnvironmentRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = new AddEnvironmentCommand(
                            new InfrastructureConfigId(id),
                            request.Name,
                            request.ShortName,
                            request.Prefix,
                            request.Suffix,
                            request.Location,
                            request.TenantId,
                            request.SubscriptionId,
                            request.Order,
                            request.RequiresApproval,
                            request.Tags.Select(t => (t.Name, t.Value)).ToList()
                        );
                        var result = await mediator.Send(command);

                        return result.Match(
                            env =>
                            {
                                var response = mapper.Map<EnvironmentDefinitionResponse>(env);
                                return Results.Created($"/infra-config/{id}/environments/{response.Id}", response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("AddEnvironment")
                .WithSummary("Add an environment definition")
                .WithDescription("Adds a new target environment (e.g. Dev, Staging, Production) to an Infrastructure Configuration. Requires Owner or Contributor access.")
                .Produces<EnvironmentDefinitionResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            environments.MapPut("/{envId:guid}",
                    async ([FromRoute] Guid id, [FromRoute] Guid envId, UpdateEnvironmentRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = new UpdateEnvironmentCommand(
                            new InfrastructureConfigId(id),
                            new EnvironmentDefinitionId(envId),
                            request.Name,
                            request.ShortName,
                            request.Prefix,
                            request.Suffix,
                            request.Location,
                            request.TenantId,
                            request.SubscriptionId,
                            request.Order,
                            request.RequiresApproval,
                            request.Tags.Select(t => (t.Name, t.Value)).ToList()
                        );
                        var result = await mediator.Send(command);

                        return result.Match(
                            env => Results.Ok(mapper.Map<EnvironmentDefinitionResponse>(env)),
                            errors => errors.Result()
                        );
                    })
                .WithName("UpdateEnvironment")
                .WithSummary("Update an environment definition")
                .WithDescription("Updates all fields of an existing environment definition. All fields are replaced — partial updates are not supported. Requires Owner or Contributor access.")
                .Produces<EnvironmentDefinitionResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            environments.MapDelete("/{envId:guid}",
                    async ([FromRoute] Guid id, [FromRoute] Guid envId, IMediator mediator) =>
                    {
                        var command = new RemoveEnvironmentCommand(
                            new InfrastructureConfigId(id),
                            new EnvironmentDefinitionId(envId)
                        );
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("RemoveEnvironment")
                .WithSummary("Remove an environment definition")
                .WithDescription("Permanently removes an environment definition from an Infrastructure Configuration. Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}
