using FluentValidation;

namespace InfraFlowSculptor.Application.ApplicationInsights.Commands.UpdateApplicationInsights;

/// <summary>
/// Validates the <see cref="UpdateApplicationInsightsCommand"/> before it is handled.
/// </summary>
public sealed class UpdateApplicationInsightsCommandValidator : AbstractValidator<UpdateApplicationInsightsCommand>
{
    /// <summary>Initializes validation rules for updating an Application Insights resource.</summary>
    public UpdateApplicationInsightsCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.LogAnalyticsWorkspaceId)
            .NotEmpty().WithMessage("LogAnalyticsWorkspaceId is required.");
    }
}
