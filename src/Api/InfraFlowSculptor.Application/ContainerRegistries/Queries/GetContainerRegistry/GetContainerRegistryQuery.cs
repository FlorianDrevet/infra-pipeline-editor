using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.ContainerRegistries.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.ContainerRegistries.Queries.GetContainerRegistry;

/// <summary>Query to retrieve a single Container Registry resource by identifier.</summary>
public record GetContainerRegistryQuery(
    AzureResourceId Id
) : IQuery<ContainerRegistryResult>;
