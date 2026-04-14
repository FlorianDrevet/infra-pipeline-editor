using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.AppServicePlans.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.AppServicePlans.Commands.UpdateAppServicePlan;

/// <summary>Command to update an existing App Service Plan.</summary>
public record UpdateAppServicePlanCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    string OsType,
    IReadOnlyList<AppServicePlanEnvironmentConfigData>? EnvironmentSettings = null
) : ICommand<AppServicePlanResult>;
