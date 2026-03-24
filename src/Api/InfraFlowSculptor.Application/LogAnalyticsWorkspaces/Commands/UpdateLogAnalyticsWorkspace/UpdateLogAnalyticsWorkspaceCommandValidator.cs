using FluentValidation;

namespace InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.UpdateLogAnalyticsWorkspace;

/// <summary>
/// Validates the <see cref="UpdateLogAnalyticsWorkspaceCommand"/> before it is handled.
/// </summary>
public sealed class UpdateLogAnalyticsWorkspaceCommandValidator : AbstractValidator<UpdateLogAnalyticsWorkspaceCommand>
{
    /// <summary>Initializes validation rules for updating a Log Analytics Workspace.</summary>
    public UpdateLogAnalyticsWorkspaceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");
    }
}
