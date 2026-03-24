using ErrorOr;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Queries.GetLogAnalyticsWorkspace;

/// <summary>Query to retrieve a single Log Analytics Workspace resource by identifier.</summary>
public record GetLogAnalyticsWorkspaceQuery(
    AzureResourceId Id
) : IRequest<ErrorOr<LogAnalyticsWorkspaceResult>>;
