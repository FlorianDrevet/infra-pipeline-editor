using FluentValidation;

namespace InfraFlowSculptor.Application.CosmosDbs.Commands.CreateCosmosDb;

/// <summary>
/// Validates the <see cref="CreateCosmosDbCommand"/> before it is handled.
/// </summary>
public sealed class CreateCosmosDbCommandValidator : AbstractValidator<CreateCosmosDbCommand>
{
    /// <summary>Initializes validation rules for creating a Cosmos DB account.</summary>
    public CreateCosmosDbCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.ResourceGroupId)
            .NotEmpty().WithMessage("ResourceGroupId is required.");
    }
}
