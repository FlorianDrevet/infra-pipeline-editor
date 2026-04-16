using InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveResourceNamingTemplate;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetDefaultNamingTemplate;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetResourceNamingTemplate;
using MediatR;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

public static class NamingTemplateController
{
    public static IEndpointRouteBuilder MapNamingTemplateEndpoints(this IEndpointRouteBuilder app)
    {
        var naming = app.MapGroup("/infra-config/{id:guid}/naming")
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
        return app;
    }
}
