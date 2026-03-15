using InfraFlowSculptor.Application.InfrastructureConfig.Commands;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddMember;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.CreateInfraConfig;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.GenerateBicep;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveMember;
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

            // POST generate-bicep
            config.MapPost("generate-bicep",
                    async (GenerateBicepRequest request, IMediator mediator) =>
                    {
                        var command = new GenerateBicepCommand(request.InfrastructureConfigId);
                        var result = await mediator.Send(command);

                        return result.Match(
                            bicepUri =>
                            {
                                var response = new GeneratedBicepResponse(bicepUri);
                                return Results.Created($"", response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("GenerateBicep");

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
        });
    }
}