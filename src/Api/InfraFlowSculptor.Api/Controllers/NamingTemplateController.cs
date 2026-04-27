using InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveResourceAbbreviationOverride;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveResourceNamingTemplate;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetDefaultNamingTemplate;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetResourceAbbreviationOverride;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetResourceNamingTemplate;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.CheckResourceNameAvailability;
using MediatR;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

public static class NamingTemplateController
{
    public static IApplicationBuilder UseNamingTemplateController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var naming = endpoints.MapGroup("/infra-config/{id:guid}/naming")
                .WithTags("Naming Templates");

            naming.MapPut("/default",
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
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            naming.MapPut("/resources/{resourceType}",
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
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            naming.MapDelete("/resources/{resourceType}",
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
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // ── Abbreviation Overrides ────────────────────────────────

            naming.MapPut("/abbreviations/{resourceType}",
                    async ([FromRoute] Guid id, [FromRoute] string resourceType, SetResourceAbbreviationOverrideRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = new SetResourceAbbreviationOverrideCommand(
                            new InfrastructureConfigId(id),
                            resourceType,
                            request.Abbreviation
                        );
                        var result = await mediator.Send(command);

                        return result.Match(
                            abbr => Results.Ok(mapper.Map<ResourceAbbreviationOverrideResponse>(abbr)),
                            errors => errors.Result()
                        );
                    })
                .WithName("SetResourceAbbreviationOverride")
                .WithSummary("Set a per-resource-type abbreviation override")
                .WithDescription(
                    "Creates or replaces the abbreviation for a specific Azure resource type (e.g. 'KeyVault' → 'kv'). " +
                    "This override takes precedence over the catalog default. " +
                    "Must be lowercase alphanumeric, max 10 characters. Requires Owner or Contributor access.")
                .Produces<ResourceAbbreviationOverrideResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            naming.MapDelete("/abbreviations/{resourceType}",
                    async ([FromRoute] Guid id, [FromRoute] string resourceType, IMediator mediator) =>
                    {
                        var command = new RemoveResourceAbbreviationOverrideCommand(
                            new InfrastructureConfigId(id),
                            resourceType
                        );
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("RemoveResourceAbbreviationOverride")
                .WithSummary("Remove a per-resource-type abbreviation override")
                .WithDescription("Removes the abbreviation override for a specific Azure resource type. The catalog default will be used instead. Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            var nameCheck = endpoints.MapGroup("/naming")
                .WithTags("Naming Templates");

            nameCheck.MapPost("/check-availability/{resourceType}",
                    async ([FromRoute] string resourceType,
                           CheckResourceNameAvailabilityRequest request,
                           IMediator mediator,
                           IMapper mapper) =>
                    {
                        if (!Guid.TryParse(request.ProjectId, out var projectGuid))
                            return Results.BadRequest("Invalid ProjectId");

                        InfrastructureConfigId? configId = null;
                        if (!string.IsNullOrWhiteSpace(request.ConfigId))
                        {
                            if (!Guid.TryParse(request.ConfigId, out var configGuid))
                                return Results.BadRequest("Invalid ConfigId");
                            configId = new InfrastructureConfigId(configGuid);
                        }

                        var query = new CheckResourceNameAvailabilityQuery(
                            new ProjectId(projectGuid),
                            configId,
                            resourceType,
                            request.Name,
                            request.CurrentPersistedName);

                        var result = await mediator.Send(query);

                        return result.Match(
                            ok => Results.Ok(mapper.Map<CheckResourceNameAvailabilityResponse>(ok)),
                            errors => errors.Result()
                        );
                    })
                .WithName("CheckResourceNameAvailability")
                .WithSummary("Check Azure resource name availability across all environments")
                .WithDescription("Applies the project/config naming templates per environment, validates the generated names against Azure naming rules, and (for supported types like ContainerRegistry) calls Azure to check global DNS availability.")
                .Produces<CheckResourceNameAvailabilityResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .ProducesProblem(StatusCodes.Status404NotFound);
        });
    }
}
