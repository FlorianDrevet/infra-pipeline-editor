using FluentValidation;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetDefaultNamingTemplate;

public class SetDefaultNamingTemplateCommandValidator : AbstractValidator<SetDefaultNamingTemplateCommand>
{
    public SetDefaultNamingTemplateCommandValidator()
    {
        // Template is optional — null means "clear the default"
        When(x => x.Template is not null, () =>
        {
            RuleFor(x => x.Template!)
                .MaximumLength(500)
                .Must(NamingTemplateValidator.HasValidStaticChars)
                .WithMessage(
                    "The template contains invalid characters. Only alphanumeric characters, hyphens, and underscores are allowed outside placeholders.")
                .Must(t => NamingTemplateValidator.GetUnknownPlaceholders(t).Count == 0)
                .WithMessage(t =>
                {
                    var unknown = NamingTemplateValidator.GetUnknownPlaceholders(t.Template!);
                    return $"Unknown placeholder(s): {string.Join(", ", unknown.Select(p => $"{{{p}}}"))}. " +
                           $"Allowed placeholders are: {string.Join(", ", NamingTemplateValidator.AllowedPlaceholders.Select(p => $"{{{p}}}"))}";
                });
        });
    }
}
