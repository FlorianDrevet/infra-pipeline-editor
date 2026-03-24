using InfraFlowSculptor.Application.InfrastructureConfig.Commands.CreateInfraConfig;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.DeleteInfraConfig;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetInheritance;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetInfraConfig;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListMyInfraConfigs;
using InfraFlowSculptor.Application.ResourceGroups.Queries.ListResourceGroupsByConfig;
using MediatR;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;
using InfraFlowSculptor.Contracts.ResourceGroups.Responses;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

public static class InfrastructureConfigController
{
    public static IApplicationBuilder UseInfrastructureConfigController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var config = endpoints.MapGroup("/infra-config")
                .WithTags("Infrastructure Configuration");

            // ── Core CRUD ────────────────────────────────────────────────────

            config.MapGet("",
                    async (IMediator mediator, IMapper mapper) =>
                    {
                        var query = new ListMyInfrastructureConfigsQuery();
                        var result = await mediator.Send(query);

                        return result.Match(
                            configs =>
                            {
                                var responses = configs.Select(c => mapper.Map<InfrastructureConfigResponse>(c)).ToList();
                                return TypedResults.Ok(responses);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("ListMyInfrastructureConfigs")
                .WithSummary("List my Infrastructure Configurations")
                .WithDescription("Returns all Infrastructure Configurations the current user has access to via project membership.")
                .Produces<IReadOnlyList<InfrastructureConfigResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized);

            config.MapGet("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new GetInfrastructureConfigQuery(new InfrastructureConfigId(id));
                        var result = await mediator.Send(query);

                        return result.Match(
                            resourceGroup =>
                            {
                                var response = mapper.Map<InfrastructureConfigResponse>(resourceGroup);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("GetInfrastructureConfiguration")
                .WithSummary("Get an Infrastructure Configuration")
                .WithDescription("Returns the full details of a single Infrastructure Configuration.")
                .Produces<InfrastructureConfigResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized);

            // GET /{id:guid}/resource-groups
            config.MapGet("/{id:guid}/resource-groups",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new ListResourceGroupsByConfigQuery(new InfrastructureConfigId(id));
                        var result = await mediator.Send(query);

                        return result.Match(
                            resourceGroups =>
                            {
                                var responses = resourceGroups
                                    .Select(rg => mapper.Map<ResourceGroupResponse>(rg))
                                    .ToList();
                                return TypedResults.Ok(responses);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("ListResourceGroupsByConfig")
                .WithSummary("List resource groups for a configuration")
                .WithDescription("Returns all Resource Groups that belong to the specified Infrastructure Configuration.")
                .Produces<IReadOnlyList<ResourceGroupResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // POST ""
            config.MapPost("",
                    async (CreateInfrastructureConfigRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = new CreateInfrastructureConfigCommand(
                            request.Name,
                            Guid.Parse(request.ProjectId));
                        var result = await mediator.Send(command);

                        return result.Match(
                            infraConfig =>
                            {
                                var response = mapper.Map<InfrastructureConfigResponse>(infraConfig);
                                return Results.Created($"/infra-config/{response.Id}", response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("CreateInfrastructureConfig")
                .WithSummary("Create an Infrastructure Configuration")
                .WithDescription("Creates a new Infrastructure Configuration within the specified project. Requires Contributor or Owner access to the project.")
                .Produces<InfrastructureConfigResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized);

            // PUT /{id:guid}/inheritance
            config.MapPut("/{id:guid}/inheritance",
                    async ([FromRoute] Guid id, SetInheritanceRequest request, IMediator mediator) =>
                    {
                        var command = new SetInheritanceCommand(
                            new InfrastructureConfigId(id),
                            request.UseProjectEnvironments,
                            request.UseProjectNamingConventions
                        );
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("SetInheritance")
                .WithSummary("Toggle project-level inheritance")
                .WithDescription("Controls whether this configuration inherits environments and/or naming conventions from the parent project. Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // DELETE /{id:guid}
            config.MapDelete("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator) =>
                    {
                        var command = new DeleteInfrastructureConfigCommand(
                            new InfrastructureConfigId(id));
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("DeleteInfrastructureConfig")
                .WithSummary("Delete an infrastructure configuration")
                .WithDescription("Permanently deletes an infrastructure configuration. Requires Owner access on the parent project.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}
