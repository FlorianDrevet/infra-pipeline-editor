using FluentValidation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.DownloadBootstrap;

/// <summary>Validates the <see cref="DownloadBootstrapCommand"/> before it is handled.</summary>
public sealed class DownloadBootstrapCommandValidator : AbstractValidator<DownloadBootstrapCommand>
{
    /// <summary>Initializes validation rules for downloading generated bootstrap artifacts.</summary>
    public DownloadBootstrapCommandValidator()
    {
        RuleFor(x => x.InfrastructureConfigId)
            .NotEmpty().WithMessage("InfrastructureConfigId is required.");
    }
}
