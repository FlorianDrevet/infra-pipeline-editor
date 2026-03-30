using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.AppServicePlans.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.AppServicePlans.Queries;

/// <summary>Query to retrieve an App Service Plan by its identifier.</summary>
public record GetAppServicePlanQuery(
    AzureResourceId Id
) : IQuery<AppServicePlanResult>;
