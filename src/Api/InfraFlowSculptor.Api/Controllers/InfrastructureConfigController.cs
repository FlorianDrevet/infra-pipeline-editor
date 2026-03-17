using InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddMember;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.CreateInfraConfig;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveMember;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.UpdateMemberRole;
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
using Shared.Api.Errors;

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
                .WithDescription("Returns all Infrastructure Configurations the current user is a member of.")
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
                .WithDescription("Returns the full details of a single Infrastructure Configuration, including members, environments, and naming templates.")
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
                        var command = new CreateInfrastructureConfigCommand(request.Name);
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
                .WithDescription("Creates a new Infrastructure Configuration. The current user is automatically added as Owner.")
                .Produces<InfrastructureConfigResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized);

            // ── Members ───────────────────────────────────────────────────────

            config.MapPost("/{id:guid}/members",
                    async ([FromRoute] Guid id, AddMemberRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = new AddMemberCommand(
                            new InfrastructureConfigId(id),
                            request.UserId,
                            request.Role);
                        var result = await mediator.Send(command);

                        return result.Match(
                            infraConfig =>
                            {
                                var response = mapper.Map<InfrastructureConfigResponse>(infraConfig);
                                return Results.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("AddMember")
                .WithSummary("Add a member")
                .WithDescription("Adds a user to an Infrastructure Configuration with the specified role. Requires Owner or Contributor access.")
                .Produces<InfrastructureConfigResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            config.MapPut("/{id:guid}/members/{userId:guid}",
                    async ([FromRoute] Guid id, [FromRoute] Guid userId, UpdateMemberRoleRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = new UpdateMemberRoleCommand(
                            new InfrastructureConfigId(id),
                            userId,
                            request.NewRole);
                        var result = await mediator.Send(command);

                        return result.Match(
                            infraConfig =>
                            {
                                var response = mapper.Map<InfrastructureConfigResponse>(infraConfig);
                                return Results.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("UpdateMemberRole")
                .WithSummary("Update a member's role")
                .WithDescription("Changes the role assigned to a member of an Infrastructure Configuration. Requires Owner access.")
                .Produces<InfrastructureConfigResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            config.MapDelete("/{id:guid}/members/{userId:guid}",
                    async ([FromRoute] Guid id, [FromRoute] Guid userId, IMediator mediator) =>
                    {
                        var command = new RemoveMemberCommand(
                            new InfrastructureConfigId(id),
                            userId);
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("RemoveMember")
                .WithSummary("Remove a member")
                .WithDescription("Removes a user from an Infrastructure Configuration. Requires Owner access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}
