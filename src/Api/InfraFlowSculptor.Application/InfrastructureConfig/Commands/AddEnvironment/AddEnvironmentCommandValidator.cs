using FluentValidation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddEnvironment;

public class AddEnvironmentCommandValidator : AbstractValidator<AddEnvironmentCommand>
{
    public AddEnvironmentCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Prefix).NotNull().MaximumLength(50);
        RuleFor(x => x.Suffix).NotNull().MaximumLength(50);
        RuleFor(x => x.Location).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.Order).GreaterThanOrEqualTo(0);

        RuleForEach(x => x.Tags).ChildRules(tag =>
        {
            tag.RuleFor(t => t.Name)
                .NotEmpty()
                .MaximumLength(100);

            tag.RuleFor(t => t.Value)
                .NotEmpty()
                .MaximumLength(100);
        });
    }
}
