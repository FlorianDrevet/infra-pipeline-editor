using ErrorOr;
using InfraFlowSculptor.Application.WebApps.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.WebApps.Commands.UpdateWebApp;

/// <summary>Command to update an existing Web App.</summary>
public record UpdateWebAppCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    Guid AppServicePlanId,
    string RuntimeStack,
    string RuntimeVersion,
    bool AlwaysOn,
    bool HttpsOnly,
    IReadOnlyList<WebAppEnvironmentConfigData>? EnvironmentSettings = null
) : IRequest<ErrorOr<WebAppResult>>;
