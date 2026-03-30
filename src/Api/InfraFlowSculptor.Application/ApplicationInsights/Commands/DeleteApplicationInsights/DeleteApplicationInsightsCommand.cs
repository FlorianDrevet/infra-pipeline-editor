using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.ApplicationInsights.Commands.DeleteApplicationInsights;

/// <summary>Command to permanently delete an Application Insights resource.</summary>
public record DeleteApplicationInsightsCommand(
    AzureResourceId Id
) : ICommand<Deleted>;
