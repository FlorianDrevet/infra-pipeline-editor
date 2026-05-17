using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectResourceNamingTemplate;

/// <summary>Validates the <see cref="RemoveProjectResourceNamingTemplateCommand"/> before it is handled.</summary>
public sealed class RemoveProjectResourceNamingTemplateCommandValidator : AbstractValidator<RemoveProjectResourceNamingTemplateCommand>
{
    /// <summary>Initializes validation rules for removing a project resource naming template.</summary>
    public RemoveProjectResourceNamingTemplateCommandValidator()
    {
        RuleFor(x => x.ProjectId.Value)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.ResourceType)
            .NotEmpty().WithMessage("ResourceType is required.")
            .MaximumLength(100).WithMessage("ResourceType must not exceed 100 characters.");
    }
}
