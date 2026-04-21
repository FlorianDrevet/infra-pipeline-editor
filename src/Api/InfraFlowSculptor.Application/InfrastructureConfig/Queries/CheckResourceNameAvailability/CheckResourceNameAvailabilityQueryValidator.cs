using FluentValidation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.CheckResourceNameAvailability;

/// <summary>Validator for <see cref="CheckResourceNameAvailabilityQuery"/>.</summary>
public sealed class CheckResourceNameAvailabilityQueryValidator
    : AbstractValidator<CheckResourceNameAvailabilityQuery>
{
    /// <summary>Initializes a new instance of the <see cref="CheckResourceNameAvailabilityQueryValidator"/> class.</summary>
    public CheckResourceNameAvailabilityQueryValidator()
    {
        RuleFor(x => x.ResourceType)
            .NotEmpty().WithMessage("ResourceType is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
    }
}
