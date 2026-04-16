using InfraFlowSculptor.Application.AppConfigurations.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.AppConfigurations.Queries.ListAppConfigurationKeys;

/// <summary>Query to list all configuration keys on an App Configuration resource.</summary>
/// <param name="AppConfigurationId">Identifier of the App Configuration resource.</param>
public record ListAppConfigurationKeysQuery(AzureResourceId AppConfigurationId)
    : IQuery<IReadOnlyCollection<AppConfigurationKeyResult>>;
