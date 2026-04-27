using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.AppConfigurations.Common;

/// <summary>
/// Application-layer result DTO for the App Configuration aggregate.
/// </summary>
public record AppConfigurationResult(
    AzureResourceId Id,
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    IReadOnlyList<AppConfigurationEnvironmentConfigData> EnvironmentSettings,

    bool IsExisting = false

);
