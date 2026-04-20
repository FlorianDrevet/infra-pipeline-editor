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
            .Must(t => NamingTemplateValidator.GetUnknownPlaceholders(t).Count == 0)
            .WithMessage(t =>
            {
                var unknown = NamingTemplateValidator.GetUnknownPlaceholders(t.Template);
                return $"Unknown placeholder(s): {string.Join(", ", unknown.Select(p => $"{{{p}}}"))}. " +
                       $"Allowed placeholders are: {string.Join(", ", NamingTemplateValidator.AllowedPlaceholders.Select(p => $"{{{p}}}"))}";
            });

        RuleFor(x => x)
            .Must(cmd => NamingTemplateValidator.HasValidStaticCharsForResourceType(cmd.Template, cmd.ResourceType))
            .WithMessage(cmd =>
            {
                var description = NamingTemplateValidator.GetAllowedCharsDescription(cmd.ResourceType);
                return description is not null
                    ? $"The template contains characters not allowed for {cmd.ResourceType}. Allowed: {description}."
                    : "The template contains invalid characters. Only alphanumeric characters, hyphens, and underscores are allowed outside placeholders.";
            })
            .When(cmd => !string.IsNullOrEmpty(cmd.Template));
    }
}
