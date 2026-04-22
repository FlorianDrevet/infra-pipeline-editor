using InfraFlowSculptor.Domain.AppServicePlanAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.AppServicePlans.Common;

/// <summary>Application-layer result for an App Service Plan operation.</summary>
public record AppServicePlanResult(
    AzureResourceId Id,
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    string OsType,
    IReadOnlyList<AppServicePlanEnvironmentConfigData> EnvironmentSettings,
    bool IsExisting = false
);
