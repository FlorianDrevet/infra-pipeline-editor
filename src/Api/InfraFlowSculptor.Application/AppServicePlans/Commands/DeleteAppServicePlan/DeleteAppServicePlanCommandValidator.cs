using FluentValidation;

namespace InfraFlowSculptor.Application.AppServicePlans.Commands.DeleteAppServicePlan;

/// <summary>Validates the <see cref="DeleteAppServicePlanCommand"/>.</summary>
public sealed class DeleteAppServicePlanCommandValidator : AbstractValidator<DeleteAppServicePlanCommand>
{
    public DeleteAppServicePlanCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}
