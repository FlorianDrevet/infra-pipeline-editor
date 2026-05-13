using FluentValidation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.DownloadBicep;

/// <summary>Validates the <see cref="DownloadBicepCommand"/> before it is handled.</summary>
public sealed class DownloadBicepCommandValidator : AbstractValidator<DownloadBicepCommand>
{
    /// <summary>Initializes validation rules for downloading generated Bicep artifacts.</summary>
    public DownloadBicepCommandValidator()
    {
        RuleFor(x => x.InfrastructureConfigId)
            .NotEmpty().WithMessage("InfrastructureConfigId is required.");
    }
}