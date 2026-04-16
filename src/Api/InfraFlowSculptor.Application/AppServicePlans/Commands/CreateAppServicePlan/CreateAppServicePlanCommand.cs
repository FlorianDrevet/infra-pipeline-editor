using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.AppServicePlans.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using ErrorOr;

namespace InfraFlowSculptor.Application.AppServicePlans.Commands.CreateAppServicePlan;

/// <summary>Command to create a new App Service Plan inside a Resource Group.</summary>
public record CreateAppServicePlanCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    string OsType,
    IReadOnlyCollection<AppServicePlanEnvironmentConfigData>? EnvironmentSettings = null
) : ICommand<AppServicePlanResult>;
