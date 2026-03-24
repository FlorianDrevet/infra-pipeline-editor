using ErrorOr;
using InfraFlowSculptor.Application.WebApps.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.WebApps.Queries;

/// <summary>Query to retrieve a Web App by its identifier.</summary>
public record GetWebAppQuery(
    AzureResourceId Id
) : IRequest<ErrorOr<WebAppResult>>;
