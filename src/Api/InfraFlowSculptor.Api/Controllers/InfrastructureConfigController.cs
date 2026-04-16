using InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddCrossConfigReference;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.CreateInfraConfig;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.DeleteInfraConfig;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveCrossConfigReference;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetInfraConfigTags;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetInheritance;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetInfraConfig;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListCrossConfigReferences;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListIncomingCrossConfigReferences;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListMyInfraConfigs;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetConfigDiagnostics;
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
    public static IEndpointRouteBuilder MapInfrastructureConfigEndpoints(this IEndpointRouteBuilder app)
    {
        var config = app.MapGroup("/infra-config")
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
            .ProducesProblem(StatusCodes.Status401Unauthorized)
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
            .WithDescription("Controls whether this configuration inherits naming conventions from the parent project. Requires Owner or Contributor access.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        // PUT /{id:guid}/tags
        config.MapPut("/{id:guid}/tags",
                async ([FromRoute] Guid id, [FromBody] SetInfraConfigTagsRequest request, ISender sender) =>
                {
                    var command = new SetInfraConfigTagsCommand(
                        id,
                        request.Tags.Select(t => (t.Name, t.Value)).ToList());
                    var result = await sender.Send(command);

                    return result.Match(
                        _ => Results.NoContent(),
                        errors => errors.Result()
                    );
                })
            .WithName("SetInfraConfigTags")
            .WithSummary("Set configuration-level tags")
            .WithDescription("Replaces all configuration-level tags with the provided set. These tags extend or override project-level tags. Requires Owner or Contributor access.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
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
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        // ── Cross-Config References ──────────────────────────────────────

        // GET /{id:guid}/cross-config-references
        config.MapGet("/{id:guid}/cross-config-references",
                async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                {
                    var query = new ListCrossConfigReferencesQuery(id);
                    var result = await mediator.Send(query);

                    return result.Match(
                        refs =>
                        {
                            var responses = refs.Select(r => mapper.Map<CrossConfigReferenceResponse>(r)).ToList();
                            return TypedResults.Ok(responses);
                        },
                        errors => errors.Result()
                    );
                })
            .WithName("ListCrossConfigReferences")
            .WithSummary("List cross-config references")
            .WithDescription("Returns all cross-configuration resource references for the specified infrastructure configuration, with resolved target metadata.")
            .Produces<IReadOnlyList<CrossConfigReferenceResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        // POST /{id:guid}/cross-config-references
        config.MapPost("/{id:guid}/cross-config-references",
                async ([FromRoute] Guid id, AddCrossConfigReferenceRequest request, IMediator mediator) =>
                {
                    var command = new AddCrossConfigReferenceCommand(id, request.TargetResourceId);
                    var result = await mediator.Send(command);

                    return result.Match(
                        reference => Results.Created(
                            $"/infra-config/{id}/cross-config-references/{reference.ReferenceId}",
                            reference),
                        errors => errors.Result()
                    );
                })
            .WithName("AddCrossConfigReference")
            .WithSummary("Add a cross-config resource reference")
            .WithDescription("Adds a reference to an Azure resource from another infrastructure configuration within the same project. Used for Bicep 'existing' resource declarations.")
            .Produces<CrossConfigReferenceResult>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        // DELETE /{id:guid}/cross-config-references/{refId:guid}
        config.MapDelete("/{id:guid}/cross-config-references/{refId:guid}",
                async ([FromRoute] Guid id, [FromRoute] Guid refId, IMediator mediator) =>
                {
                    var command = new RemoveCrossConfigReferenceCommand(id, refId);
                    var result = await mediator.Send(command);

                    return result.Match(
                        _ => Results.NoContent(),
                        errors => errors.Result()
                    );
                })
            .WithName("RemoveCrossConfigReference")
            .WithSummary("Remove a cross-config resource reference")
            .WithDescription("Removes a cross-configuration resource reference from the infrastructure configuration.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        // GET /{id:guid}/incoming-cross-config-references
        config.MapGet("/{id:guid}/incoming-cross-config-references",
                async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                {
                    var query = new ListIncomingCrossConfigReferencesQuery(id);
                    var result = await mediator.Send(query);

                    return result.Match(
                        refs =>
                        {
                            var responses = refs.Select(r => mapper.Map<IncomingCrossConfigReferenceResponse>(r)).ToList();
                            return TypedResults.Ok(responses);
                        },
                        errors => errors.Result()
                    );
                })
            .WithName("ListIncomingCrossConfigReferences")
            .WithSummary("List incoming cross-config references")
            .WithDescription("Returns resources from other configurations in the same project that depend on resources in this configuration.")
            .Produces<IReadOnlyList<IncomingCrossConfigReferenceResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        // GET /{id:guid}/diagnostics
        config.MapGet("/{id:guid}/diagnostics",
                async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                {
                    var query = new GetConfigDiagnosticsQuery(id);
                    var result = await mediator.Send(query);

                    return result.Match(
                        r => TypedResults.Ok(mapper.Map<ConfigDiagnosticsResponse>(r)),
                        errors => errors.Result()
                    );
                })
            .WithName("GetConfigDiagnostics")
            .WithSummary("Get configuration diagnostics")
            .WithDescription("Runs all diagnostic rules against the configuration and returns findings such as missing RBAC assignments.")
            .Produces<ConfigDiagnosticsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
