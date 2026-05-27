using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.DownloadProjectBicep;

/// <summary>Validates the <see cref="DownloadProjectBicepCommand"/> before it is handled.</summary>
public sealed class DownloadProjectBicepCommandValidator : AbstractValidator<DownloadProjectBicepCommand>
{
    /// <summary>Initializes validation rules for downloading project Bicep artifacts.</summary>
    public DownloadProjectBicepCommandValidator()
    {
        RuleFor(x => x.ProjectId.Value)
            .NotEmpty().WithMessage("ProjectId is required.");
    }
}
