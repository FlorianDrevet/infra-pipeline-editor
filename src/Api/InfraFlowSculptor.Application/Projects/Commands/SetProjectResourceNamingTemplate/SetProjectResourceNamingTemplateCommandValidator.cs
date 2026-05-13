using FluentValidation;
using InfraFlowSculptor.Application.Common.Validation;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.Application.Projects.Commands.SetProjectResourceNamingTemplate;

/// <summary>Validates <see cref="SetProjectResourceNamingTemplateCommand"/>.</summary>
public sealed class SetProjectResourceNamingTemplateCommandValidator
    : AbstractValidator<SetProjectResourceNamingTemplateCommand>
{
    /// <inheritdoc />
    public SetProjectResourceNamingTemplateCommandValidator()
    {
        RuleFor(x => x.ResourceType)
            .NotEmpty()
            .MaximumLength(100)
            .Must(resourceType => AzureResourceTypes.All.Contains(resourceType))
            .WithMessage("ResourceType must be a supported Azure resource type.");

        NamingTemplateValidationRules.ApplyTemplateRules(RuleFor(x => x.Template));

        NamingTemplateValidationRules.ApplyStaticCharsRule(this,
            cmd => cmd.Template, cmd => cmd.ResourceType);
    }
}
