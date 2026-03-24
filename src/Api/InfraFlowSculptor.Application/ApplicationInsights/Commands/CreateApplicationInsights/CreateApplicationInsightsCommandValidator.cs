using FluentValidation;

namespace InfraFlowSculptor.Application.ApplicationInsights.Commands.CreateApplicationInsights;

/// <summary>
/// Validates the <see cref="CreateApplicationInsightsCommand"/> before it is handled.
/// </summary>
public sealed class CreateApplicationInsightsCommandValidator : AbstractValidator<CreateApplicationInsightsCommand>
{
    /// <summary>Initializes validation rules for creating an Application Insights resource.</summary>
    public CreateApplicationInsightsCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.ResourceGroupId)
            .NotEmpty().WithMessage("ResourceGroupId is required.");

        RuleFor(x => x.LogAnalyticsWorkspaceId)
            .NotEmpty().WithMessage("LogAnalyticsWorkspaceId is required.");
    }
}
