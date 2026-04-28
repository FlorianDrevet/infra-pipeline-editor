using FluentValidation;
using InfraFlowSculptor.Application.Common.Validation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetResourceNamingTemplate;

public class SetResourceNamingTemplateCommandValidator : AbstractValidator<SetResourceNamingTemplateCommand>
{
    public SetResourceNamingTemplateCommandValidator()
    {
        RuleFor(x => x.ResourceType).NotEmpty().MaximumLength(100);

        NamingTemplateValidationRules.ApplyTemplateRules(RuleFor(x => x.Template));

        NamingTemplateValidationRules.ApplyStaticCharsRule(this,
            cmd => cmd.Template, cmd => cmd.ResourceType);
    }
}
