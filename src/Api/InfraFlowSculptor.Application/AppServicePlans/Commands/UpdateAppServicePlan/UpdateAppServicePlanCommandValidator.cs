using FluentValidation;
using InfraFlowSculptor.Domain.AppServicePlanAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.AppServicePlans.Commands.UpdateAppServicePlan;

/// <summary>Validates the <see cref="UpdateAppServicePlanCommand"/> before it is handled.</summary>
public sealed class UpdateAppServicePlanCommandValidator : AbstractValidator<UpdateAppServicePlanCommand>
{
    /// <summary>Initializes validation rules for updating an App Service Plan.</summary>
    public UpdateAppServicePlanCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

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
