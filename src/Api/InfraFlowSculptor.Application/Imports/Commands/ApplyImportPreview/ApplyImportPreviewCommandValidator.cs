using FluentValidation;

namespace InfraFlowSculptor.Application.Imports.Commands.ApplyImportPreview;

/// <summary>
/// Validates the <see cref="ApplyImportPreviewCommand"/> before it is handled.
/// </summary>
public sealed class ApplyImportPreviewCommandValidator : AbstractValidator<ApplyImportPreviewCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplyImportPreviewCommandValidator"/> class.
    /// </summary>
    public ApplyImportPreviewCommandValidator()
    {
        RuleFor(command => command.ProjectName)
            .NotEmpty().WithMessage("ProjectName is required.")
            .Length(3, 80).WithMessage("ProjectName must be between 3 and 80 characters.");

        RuleFor(command => command.LayoutPreset)
            .NotEmpty().WithMessage("LayoutPreset is required.");

        RuleFor(command => command.Preview)
            .NotNull().WithMessage("Preview is required.");

        When(command => command.Preview is not null, () =>
        {
            RuleFor(command => command.Preview.SourceFormat)
                .NotEmpty().WithMessage("Preview.SourceFormat is required.");

            RuleFor(command => command.Preview.Summary)
                .NotEmpty().WithMessage("Preview.Summary is required.");
        });
    }
}