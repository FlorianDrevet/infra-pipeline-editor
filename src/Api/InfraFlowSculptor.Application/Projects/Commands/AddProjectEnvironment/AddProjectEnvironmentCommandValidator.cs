using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.AddProjectEnvironment;

/// <summary>Validates the <see cref="AddProjectEnvironmentCommand"/>.</summary>
public sealed class AddProjectEnvironmentCommandValidator : AbstractValidator<AddProjectEnvironmentCommand>
{
    public AddProjectEnvironmentCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Prefix).NotNull().MaximumLength(50);
        RuleFor(x => x.Suffix).NotNull().MaximumLength(50);
        RuleFor(x => x.Location).NotEmpty();
        // SubscriptionId is optional at creation time: Guid.Empty means "to be configured later".
        RuleFor(x => x.Order).GreaterThanOrEqualTo(0);

        RuleForEach(x => x.Tags).ChildRules(tag =>
        {
            tag.RuleFor(t => t.Name).NotEmpty().MaximumLength(100);
            tag.RuleFor(t => t.Value).NotEmpty().MaximumLength(100);
        });
    }
}
