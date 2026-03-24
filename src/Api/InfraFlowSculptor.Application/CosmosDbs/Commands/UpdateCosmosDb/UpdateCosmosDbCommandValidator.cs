using FluentValidation;

namespace InfraFlowSculptor.Application.CosmosDbs.Commands.UpdateCosmosDb;

/// <summary>
/// Validates the <see cref="UpdateCosmosDbCommand"/> before it is handled.
/// </summary>
public sealed class UpdateCosmosDbCommandValidator : AbstractValidator<UpdateCosmosDbCommand>
{
    /// <summary>Initializes validation rules for updating a Cosmos DB account.</summary>
    public UpdateCosmosDbCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");
    }
}
