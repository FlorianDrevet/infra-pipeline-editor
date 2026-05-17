using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Application.Common.Queries.GetDependentResources;
using InfraFlowSculptor.Contracts.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InfraFlowSculptor.Api.Controllers.Common;

internal static class DependentResourcesEndpointMapper
{
    internal static RouteHandlerBuilder MapDependentResourcesEndpoint(
        this RouteGroupBuilder group,
        string routeName,
        string resourceDisplayName)
    {
        return group.MapGet("/{id:guid}/dependents",
                async ([FromRoute] Guid id, IMediator mediator) =>
                {
                    var query = new GetDependentResourcesQuery(new AzureResourceId(id));
                    var result = await mediator.Send(query);

                    return result.Match(
                        dependents => Results.Ok(dependents.Select(d =>
                            new DependentResourceResponse(d.Id.ToString(), d.Name, d.ResourceType)).ToList()),
                        errors => errors.Result()
                    );
                })
            .WithName(routeName)
            .WithSummary("Get dependent resources")
            .WithDescription($"Returns all resources that depend on this {resourceDisplayName} and would be deleted alongside it.")
            .Produces<List<DependentResourceResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
