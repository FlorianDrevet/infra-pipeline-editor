using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.ApplicationInsights.Commands.DeleteApplicationInsights;

/// <summary>Command to permanently delete an Application Insights resource.</summary>
public record DeleteApplicationInsightsCommand(
    AzureResourceId Id
) : IRequest<ErrorOr<Deleted>>;
