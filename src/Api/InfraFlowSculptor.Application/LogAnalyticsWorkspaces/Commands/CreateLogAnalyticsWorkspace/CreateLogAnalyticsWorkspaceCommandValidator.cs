using FluentValidation;

namespace InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.CreateLogAnalyticsWorkspace;

/// <summary>
/// Validates the <see cref="CreateLogAnalyticsWorkspaceCommand"/> before it is handled.
/// </summary>
public sealed class CreateLogAnalyticsWorkspaceCommandValidator : AbstractValidator<CreateLogAnalyticsWorkspaceCommand>
{
    /// <summary>Initializes validation rules for creating a Log Analytics Workspace.</summary>
    public CreateLogAnalyticsWorkspaceCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.ResourceGroupId)
            .NotEmpty().WithMessage("ResourceGroupId is required.");
    }
}
