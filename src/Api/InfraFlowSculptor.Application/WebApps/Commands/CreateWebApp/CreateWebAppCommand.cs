using InfraFlowSculptor.Application.WebApps.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using MediatR;
using ErrorOr;

namespace InfraFlowSculptor.Application.WebApps.Commands.CreateWebApp;

/// <summary>Command to create a new Web App inside a Resource Group.</summary>
public record CreateWebAppCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    Guid AppServicePlanId,
    string RuntimeStack,
    string RuntimeVersion,
    bool AlwaysOn,
    bool HttpsOnly,
    IReadOnlyList<WebAppEnvironmentConfigData>? EnvironmentSettings = null
) : IRequest<ErrorOr<WebAppResult>>;
