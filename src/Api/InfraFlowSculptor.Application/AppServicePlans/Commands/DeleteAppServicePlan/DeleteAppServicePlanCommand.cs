using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.AppServicePlans.Commands.DeleteAppServicePlan;

/// <summary>Command to delete an App Service Plan.</summary>
public record DeleteAppServicePlanCommand(
    AzureResourceId Id
) : IRequest<ErrorOr<Deleted>>;
