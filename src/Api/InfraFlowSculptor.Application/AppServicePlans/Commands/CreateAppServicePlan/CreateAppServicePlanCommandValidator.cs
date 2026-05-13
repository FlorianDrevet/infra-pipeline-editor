using FluentValidation;
using InfraFlowSculptor.Domain.AppServicePlanAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.AppServicePlans.Commands.CreateAppServicePlan;

/// <summary>Validates the <see cref="CreateAppServicePlanCommand"/> before it is handled.</summary>
public sealed class CreateAppServicePlanCommandValidator : AbstractValidator<CreateAppServicePlanCommand>
{
    /// <summary>Initializes validation rules for creating an App Service Plan.</summary>
    public CreateAppServicePlanCommandValidator()
    {
        RuleFor(x => x.ResourceGroupId)
            .NotEmpty().WithMessage("ResourceGroupId is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.");

        RuleFor(x => x.OsType)
            .NotEmpty().WithMessage("OsType is required.")
            .Must(value => Enum.TryParse<AppServicePlanOsType.AppServicePlanOsTypeEnum>(value, out _))
            .WithMessage("OsType must be a valid value (Windows or Linux).");
    }
}