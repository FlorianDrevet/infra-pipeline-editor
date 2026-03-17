using InfraFlowSculptor.Application.InfrastructureConfig.Commands;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddEnvironment;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddMember;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.CreateInfraConfig;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveEnvironment;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveMember;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveResourceNamingTemplate;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetDefaultNamingTemplate;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetResourceNamingTemplate;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.UpdateEnvironment;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.UpdateMemberRole;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetInfraConfig;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListMyInfraConfigs;
using MediatR;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;
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

            // ── Environments ─────────────────────────────────────────────────

            config.MapPost("/{id:guid}/environments",
                    async ([FromRoute] Guid id, AddEnvironmentRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = new AddEnvironmentCommand(
                            new InfrastructureConfigId(id),
                            request.Name,
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

            config.MapPut("/{id:guid}/environments/{envId:guid}",
                    async ([FromRoute] Guid id, [FromRoute] Guid envId, UpdateEnvironmentRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = new UpdateEnvironmentCommand(
                            new InfrastructureConfigId(id),
                            new EnvironmentDefinitionId(envId),
                            request.Name,
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

            config.MapDelete("/{id:guid}/environments/{envId:guid}",
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

            // ── Naming Conventions ────────────────────────────────────────────

            config.MapPut("/{id:guid}/naming/default",
                    async ([FromRoute] Guid id, SetDefaultNamingTemplateRequest request, IMediator mediator) =>
                    {
                        var command = new SetDefaultNamingTemplateCommand(
                            new InfrastructureConfigId(id),
                            request.Template
                        );
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("SetDefaultNamingTemplate")
                .WithSummary("Set the default naming template")
                .WithDescription(
                    "Sets or clears the default naming template for all resource types that do not have a specific override. " +
                    "Template supports placeholders: {name}, {prefix}, {suffix}, {env}, {resourceType}, {location}. " +
                    "Send null or omit the field to clear the template. Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            config.MapPut("/{id:guid}/naming/resources/{resourceType}",
                    async ([FromRoute] Guid id, [FromRoute] string resourceType, SetResourceNamingTemplateRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = new SetResourceNamingTemplateCommand(
                            new InfrastructureConfigId(id),
                            resourceType,
                            request.Template
                        );
                        var result = await mediator.Send(command);

                        return result.Match(
                            tpl => Results.Ok(mapper.Map<ResourceNamingTemplateResponse>(tpl)),
                            errors => errors.Result()
                        );
                    })
                .WithName("SetResourceNamingTemplate")
                .WithSummary("Set a per-resource-type naming template")
                .WithDescription(
                    "Creates or replaces the naming template for a specific Azure resource type (e.g. 'KeyVault', 'StorageAccount'). " +
                    "This override takes precedence over the default naming template. " +
                    "Template supports placeholders: {name}, {prefix}, {suffix}, {env}, {resourceType}, {location}. " +
                    "Requires Owner or Contributor access.")
                .Produces<ResourceNamingTemplateResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            config.MapDelete("/{id:guid}/naming/resources/{resourceType}",
                    async ([FromRoute] Guid id, [FromRoute] string resourceType, IMediator mediator) =>
                    {
                        var command = new RemoveResourceNamingTemplateCommand(
                            new InfrastructureConfigId(id),
                            resourceType
                        );
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("RemoveResourceNamingTemplate")
                .WithSummary("Remove a per-resource-type naming template")
                .WithDescription("Removes the naming template override for a specific Azure resource type. The default naming template will be used instead. Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}
