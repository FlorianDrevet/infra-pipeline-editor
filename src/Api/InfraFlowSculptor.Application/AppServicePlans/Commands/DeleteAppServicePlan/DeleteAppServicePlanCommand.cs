using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.AppServicePlans.Commands.DeleteAppServicePlan;

/// <summary>Command to delete an App Service Plan.</summary>
public record DeleteAppServicePlanCommand(
    AzureResourceId Id
) : ICommand<Deleted>;
