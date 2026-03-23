using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.FunctionApps.Commands.DeleteFunctionApp;

/// <summary>Command to delete a Function App.</summary>
public record DeleteFunctionAppCommand(
    AzureResourceId Id
) : IRequest<ErrorOr<Deleted>>;
