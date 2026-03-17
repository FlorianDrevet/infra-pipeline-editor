using FluentValidation;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetResourceNamingTemplate;

public class SetResourceNamingTemplateCommandValidator : AbstractValidator<SetResourceNamingTemplateCommand>
{
    public SetResourceNamingTemplateCommandValidator()
    {
        RuleFor(x => x.ResourceType).NotEmpty().MaximumLength(100);

        RuleFor(x => x.Template)
            .NotEmpty()
            .MaximumLength(500)
            .Must(NamingTemplateValidator.HasValidStaticChars)
            .WithMessage(
                "The template contains invalid characters. Only alphanumeric characters, hyphens, and underscores are allowed outside placeholders.")
            .Must(t => NamingTemplateValidator.GetUnknownPlaceholders(t).Count == 0)
            .WithMessage(t =>
            {
                var unknown = NamingTemplateValidator.GetUnknownPlaceholders(t.Template);
                return $"Unknown placeholder(s): {string.Join(", ", unknown.Select(p => $"{{{p}}}"))}. " +
                       $"Allowed placeholders are: {string.Join(", ", NamingTemplateValidator.AllowedPlaceholders.Select(p => $"{{{p}}}"))}";
            });
    }
}
