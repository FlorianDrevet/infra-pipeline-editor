using ErrorOr;
using InfraFlowSculptor.Application.ApplicationInsights.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.ApplicationInsights.Queries.GetApplicationInsights;

/// <summary>Query to retrieve a single Application Insights resource by identifier.</summary>
public record GetApplicationInsightsQuery(
    AzureResourceId Id
) : IRequest<ErrorOr<ApplicationInsightsResult>>;
