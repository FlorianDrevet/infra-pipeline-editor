using FluentValidation;
using InfraFlowSculptor.Application.Common.Validation;

namespace InfraFlowSculptor.Application.Projects.Commands.SetProjectDefaultNamingTemplate;

/// <summary>Validates the <see cref="SetProjectDefaultNamingTemplateCommand"/> before it is handled.</summary>
public sealed class SetProjectDefaultNamingTemplateCommandValidator
    : AbstractValidator<SetProjectDefaultNamingTemplateCommand>
{
    /// <summary>Initializes the validator.</summary>
    public SetProjectDefaultNamingTemplateCommandValidator()
    {
        RuleFor(x => x.ProjectId.Value)
            .NotEmpty().WithMessage("ProjectId is required.");

        When(
            x => x.Template is not null,
            () =>
            {
                NamingTemplateValidationRules.ApplyTemplateRules(RuleFor(x => x.Template!));
            });
    }
}