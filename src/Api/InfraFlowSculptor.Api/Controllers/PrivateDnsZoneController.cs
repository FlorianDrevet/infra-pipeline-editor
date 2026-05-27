using InfraFlowSculptor.Api.Controllers.Constants;
using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Application.PrivateDnsZones.Commands.CreatePrivateDnsZone;
using InfraFlowSculptor.Application.PrivateDnsZones.Commands.DeletePrivateDnsZone;
using InfraFlowSculptor.Application.PrivateDnsZones.Commands.UpdatePrivateDnsZone;
using InfraFlowSculptor.Application.PrivateDnsZones.Queries;
using InfraFlowSculptor.Contracts.PrivateDnsZones.Requests;
using InfraFlowSculptor.Contracts.PrivateDnsZones.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>Minimal API endpoints for Private DNS Zone resources.</summary>
public static class PrivateDnsZoneController
{
    /// <summary>Registers Private DNS Zone endpoints.</summary>
    public static IApplicationBuilder UsePrivateDnsZoneController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup(Routes.PrivateDnsZone)
                .WithTags("PrivateDnsZones");

            group.MapGet("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new GetPrivateDnsZoneQuery(new AzureResourceId(id));
                        var result = await mediator.Send(query);
                        return result.Match(
                            zone => TypedResults.Ok(mapper.Map<PrivateDnsZoneResponse>(zone)),
                            errors => errors.Result());
                    })
                .WithName(PrivateDnsZoneRouteNames.GetPrivateDnsZone)
                .WithSummary("Get a Private DNS Zone")
                .Produces<PrivateDnsZoneResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("",
                    async (CreatePrivateDnsZoneRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<CreatePrivateDnsZoneCommand>(request);
                        var result = await mediator.Send(command);
                        return result.Match(
                            zone =>
                            {
                                var response = mapper.Map<PrivateDnsZoneResponse>(zone);
                                return TypedResults.CreatedAtRoute(
                                    routeName: PrivateDnsZoneRouteNames.GetPrivateDnsZone,
                                    routeValues: new { id = response.Id },
                                    value: response);
                            },
                            errors => errors.Result());
                    })
                .WithName(PrivateDnsZoneRouteNames.CreatePrivateDnsZone)
                .WithSummary("Create a Private DNS Zone")
                .Produces<PrivateDnsZoneResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}",
                    async ([FromRoute] Guid id, UpdatePrivateDnsZoneRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<UpdatePrivateDnsZoneCommand>((id, request));
                        var result = await mediator.Send(command);
                        return result.Match(
                            zone => TypedResults.Ok(mapper.Map<PrivateDnsZoneResponse>(zone)),
                            errors => errors.Result());
                    })
                .WithName(PrivateDnsZoneRouteNames.UpdatePrivateDnsZone)
                .WithSummary("Update a Private DNS Zone")
                .Produces<PrivateDnsZoneResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator) =>
                    {
                        var command = new DeletePrivateDnsZoneCommand(new AzureResourceId(id));
                        var result = await mediator.Send(command);
                        return result.Match(_ => Results.NoContent(), errors => errors.Result());
                    })
                .WithName(PrivateDnsZoneRouteNames.DeletePrivateDnsZone)
                .WithSummary("Delete a Private DNS Zone")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}
