using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.AppConfigurations.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.AppConfigurations.Queries;

/// <summary>Query to retrieve a single App Configuration resource by identifier.</summary>
/// <param name="Id">The App Configuration identifier.</param>
public record GetAppConfigurationQuery(
    AzureResourceId Id
) : IQuery<AppConfigurationResult>;
