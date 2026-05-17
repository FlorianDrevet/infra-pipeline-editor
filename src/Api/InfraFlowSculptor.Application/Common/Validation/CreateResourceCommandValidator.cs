using FluentValidation;
using InfraFlowSculptor.Application.Common.Interfaces;

namespace InfraFlowSculptor.Application.Common.Validation;

/// <summary>
/// Base validator for create-resource commands that enforces the common
/// <c>ResourceGroupId</c>, <c>Name</c>, and <c>Location</c> rules.
/// Concrete validators inherit this class and add resource-specific rules.
/// </summary>
/// <typeparam name="T">A command type implementing <see cref="ICreateResourceCommand"/>.</typeparam>
public abstract class CreateResourceCommandValidator<T> : AbstractValidator<T>
    where T : ICreateResourceCommand
{
    /// <summary>Registers the shared rules for resource-group id, name, and location.</summary>
    protected CreateResourceCommandValidator()
    {
        RuleFor(x => x.ResourceGroupId)
            .NotEmpty().WithMessage("ResourceGroupId is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.");
    }
}
