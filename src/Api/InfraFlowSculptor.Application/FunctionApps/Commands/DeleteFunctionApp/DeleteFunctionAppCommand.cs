using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.FunctionApps.Commands.DeleteFunctionApp;

/// <summary>Command to delete a Function App.</summary>
public record DeleteFunctionAppCommand(
    AzureResourceId Id
) : ICommand<Deleted>;
