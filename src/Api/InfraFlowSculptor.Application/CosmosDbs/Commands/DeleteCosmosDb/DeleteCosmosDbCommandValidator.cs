using FluentValidation;

namespace InfraFlowSculptor.Application.CosmosDbs.Commands.DeleteCosmosDb;

/// <summary>Validates the <see cref="DeleteCosmosDbCommand"/>.</summary>
public sealed class DeleteCosmosDbCommandValidator : AbstractValidator<DeleteCosmosDbCommand>
{
    public DeleteCosmosDbCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}
