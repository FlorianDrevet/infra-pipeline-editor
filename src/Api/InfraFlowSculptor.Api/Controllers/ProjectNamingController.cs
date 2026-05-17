using InfraFlowSculptor.Api.Controllers.Constants;
using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectResourceAbbreviation;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectResourceNamingTemplate;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectDefaultNamingTemplate;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectResourceAbbreviation;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectResourceNamingTemplate;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;
using InfraFlowSculptor.Contracts.Projects.Requests;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>Minimal API endpoint definitions for Project naming and abbreviation operations.</summary>
public static class ProjectNamingController
{
    /// <summary>Registers the Project naming endpoints on the application builder.</summary>
    public static IApplicationBuilder UseProjectNamingController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/projects")
                .WithTags("Projects");

            MapNamingAndAbbreviationEndpoints(group);
        });
    }

    private static void MapNamingAndAbbreviationEndpoints(RouteGroupBuilder group)
    {
        // ── Naming Templates ──────────────────────────────────

        group.MapPut("/{id:guid}/naming/default",
                async ([FromRoute] Guid id, SetProjectDefaultNamingTemplateRequest request, IMediator mediator) =>
                {
                    var command = new SetProjectDefaultNamingTemplateCommand(
                        new ProjectId(id),
                        request.Template
                    );
                    var result = await mediator.Send(command);

                    return result.Match(
                        _ => Results.NoContent(),
                        errors => errors.Result()
                    );
                })
            .WithName(ProjectRouteNames.SetProjectDefaultNamingTemplate)
            .WithSummary("Set the project default naming template")
            .WithDescription("Sets or clears the default naming template at the project level. Requires Owner or Contributor access.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}/naming/resources/{resourceType}",
                async ([FromRoute] Guid id, [FromRoute] string resourceType, SetProjectResourceNamingTemplateRequest request, IMediator mediator, IMapper mapper) =>
                {
                    var command = new SetProjectResourceNamingTemplateCommand(
                        new ProjectId(id),
                        resourceType,
                        request.Template
                    );
                    var result = await mediator.Send(command);

                    return result.Match(
                        tpl => Results.Ok(mapper.Map<ResourceNamingTemplateResponse>(tpl)),
                        errors => errors.Result()
                    );
                })
            .WithName(ProjectRouteNames.SetProjectResourceNamingTemplate)
            .WithSummary("Set a per-resource-type naming template")
            .WithDescription("Creates or replaces a naming template for a specific Azure resource type at the project level. Requires Owner or Contributor access.")
            .Produces<ResourceNamingTemplateResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapDelete("/{id:guid}/naming/resources/{resourceType}",
                async ([FromRoute] Guid id, [FromRoute] string resourceType, IMediator mediator) =>
                {
                    var command = new RemoveProjectResourceNamingTemplateCommand(
                        new ProjectId(id),
                        resourceType
                    );
                    var result = await mediator.Send(command);

                    return result.Match(
                        _ => Results.NoContent(),
                        errors => errors.Result()
                    );
                })
            .WithName(ProjectRouteNames.RemoveProjectResourceNamingTemplate)
            .WithSummary("Remove a per-resource-type naming template")
            .WithDescription("Removes a per-resource-type naming template from the project. Requires Owner or Contributor access.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        // ── Abbreviation Overrides ────────────────────────────

        group.MapPut("/{id:guid}/naming/abbreviations/{resourceType}",
                async ([FromRoute] Guid id, [FromRoute] string resourceType, SetResourceAbbreviationOverrideRequest request, IMediator mediator, IMapper mapper) =>
                {
                    var command = new SetProjectResourceAbbreviationCommand(
                        new ProjectId(id),
                        resourceType,
                        request.Abbreviation
                    );
                    var result = await mediator.Send(command);

                    return result.Match(
                        abbr => Results.Ok(mapper.Map<ResourceAbbreviationOverrideResponse>(abbr)),
                        errors => errors.Result()
                    );
                })
            .WithName(ProjectRouteNames.SetProjectResourceAbbreviation)
            .WithSummary("Set a per-resource-type abbreviation override")
            .WithDescription("Creates or replaces the abbreviation for a specific Azure resource type at the project level. Must be lowercase alphanumeric, max 10 characters. Requires Owner or Contributor access.")
            .Produces<ResourceAbbreviationOverrideResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapDelete("/{id:guid}/naming/abbreviations/{resourceType}",
                async ([FromRoute] Guid id, [FromRoute] string resourceType, IMediator mediator) =>
                {
                    var command = new RemoveProjectResourceAbbreviationCommand(
                        new ProjectId(id),
                        resourceType
                    );
                    var result = await mediator.Send(command);

                    return result.Match(
                        _ => Results.NoContent(),
                        errors => errors.Result()
                    );
                })
            .WithName(ProjectRouteNames.RemoveProjectResourceAbbreviation)
            .WithSummary("Remove a per-resource-type abbreviation override")
            .WithDescription("Removes the abbreviation override for a specific Azure resource type from the project. The catalog default will be used instead. Requires Owner or Contributor access.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
