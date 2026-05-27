using InfraFlowSculptor.Application.Common.Validation;

namespace InfraFlowSculptor.Application.CosmosDbs.Commands.CreateCosmosDb;

/// <summary>
/// Validates the <see cref="CreateCosmosDbCommand"/> before it is handled.
/// </summary>
public sealed class CreateCosmosDbCommandValidator : CreateResourceCommandValidator<CreateCosmosDbCommand>
{
}
