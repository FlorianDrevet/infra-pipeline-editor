using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.AppSettings.Queries.GetAvailableOutputs;

/// <summary>
/// Query to get the list of available outputs from a resource that can be used as app settings.
/// </summary>
/// <param name="ResourceId">Identifier of the source Azure resource.</param>
public record GetAvailableOutputsQuery(AzureResourceId ResourceId)
    : IQuery<AvailableOutputsResult>;
