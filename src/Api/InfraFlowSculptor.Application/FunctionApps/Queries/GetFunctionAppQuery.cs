using ErrorOr;
using InfraFlowSculptor.Application.FunctionApps.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.FunctionApps.Queries;

/// <summary>Query to retrieve a Function App by its identifier.</summary>
public record GetFunctionAppQuery(
    AzureResourceId Id
) : IRequest<ErrorOr<FunctionAppResult>>;
