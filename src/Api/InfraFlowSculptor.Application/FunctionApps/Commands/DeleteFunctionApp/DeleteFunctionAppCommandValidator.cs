using FluentValidation;

namespace InfraFlowSculptor.Application.FunctionApps.Commands.DeleteFunctionApp;

/// <summary>Validates the <see cref="DeleteFunctionAppCommand"/>.</summary>
public sealed class DeleteFunctionAppCommandValidator : AbstractValidator<DeleteFunctionAppCommand>
{
    public DeleteFunctionAppCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}
