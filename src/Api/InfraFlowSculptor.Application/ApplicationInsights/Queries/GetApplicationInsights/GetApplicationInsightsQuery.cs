using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.ApplicationInsights.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.ApplicationInsights.Queries.GetApplicationInsights;

/// <summary>Query to retrieve a single Application Insights resource by identifier.</summary>
public record GetApplicationInsightsQuery(
    AzureResourceId Id
) : IQuery<ApplicationInsightsResult>;
