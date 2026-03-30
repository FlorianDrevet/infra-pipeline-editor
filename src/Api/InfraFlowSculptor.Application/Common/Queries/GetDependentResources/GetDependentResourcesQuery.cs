using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Queries.GetDependentResources;

/// <summary>Query to retrieve all resources that depend on a given parent resource.</summary>
/// <param name="Id">Identifier of the parent resource.</param>
public record GetDependentResourcesQuery(
    AzureResourceId Id
) : IQuery<List<DependentResourceResult>>;
