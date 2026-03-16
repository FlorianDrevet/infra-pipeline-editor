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

            // GET "" - list my configs
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
                .WithName("ListMyInfrastructureConfigs");

            // GET /{id:guid}
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
                .WithName("GetInfrastructureConfiguration");

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
                .AddOpenApiOperationTransformer((operation, context, ct) =>
                {
                    operation.Summary = "Create a new Infrastructure Configuration";
                    operation.Description = "Creates a new Infrastructure Configuration with the specified name.";
                    return Task.CompletedTask;
                });

            // POST /{id:guid}/members - add a member
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
                .WithName("AddMember");

            // PUT /{id:guid}/members/{userId:guid} - update member role
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
                .WithName("UpdateMemberRole");

            // DELETE /{id:guid}/members/{userId:guid} - remove member
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
                .WithName("RemoveMember");

            // ── Environments ────────────────────────────────────────────────

            // POST /{id:guid}/environments - add an environment
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
                .WithName("AddEnvironment");

            // PUT /{id:guid}/environments/{envId:guid} - update an environment
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
                .WithName("UpdateEnvironment");

            // DELETE /{id:guid}/environments/{envId:guid} - remove an environment
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
                .WithName("RemoveEnvironment");

            // ── Naming Conventions ───────────────────────────────────────────

            // PUT /{id:guid}/naming/default - set (or clear) the default naming template
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
                .WithName("SetDefaultNamingTemplate");

            // PUT /{id:guid}/naming/resources/{resourceType} - set per-resource-type template
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
                .WithName("SetResourceNamingTemplate");

            // DELETE /{id:guid}/naming/resources/{resourceType} - remove per-resource-type template
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
                .WithName("RemoveResourceNamingTemplate");
        });
    }
}