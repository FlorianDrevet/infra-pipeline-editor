using FluentValidation;
using InfraFlowSculptor.Application.Common.Validation;

namespace InfraFlowSculptor.Application.Projects.Commands.SetProjectResourceNamingTemplate;

/// <summary>Validates <see cref="SetProjectResourceNamingTemplateCommand"/>.</summary>
public sealed class SetProjectResourceNamingTemplateCommandValidator
    : AbstractValidator<SetProjectResourceNamingTemplateCommand>
{
    /// <inheritdoc />
    public SetProjectResourceNamingTemplateCommandValidator()
    {
        RuleFor(x => x.ResourceType).NotEmpty().MaximumLength(100);

        NamingTemplateValidationRules.ApplyTemplateRules(RuleFor(x => x.Template));

        NamingTemplateValidationRules.ApplyStaticCharsRule(this,
            cmd => cmd.Template, cmd => cmd.ResourceType);
    }
}
