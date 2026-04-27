using FluentValidation;

namespace InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.DeleteLogAnalyticsWorkspace;

/// <summary>Validates the <see cref="DeleteLogAnalyticsWorkspaceCommand"/>.</summary>
public sealed class DeleteLogAnalyticsWorkspaceCommandValidator : AbstractValidator<DeleteLogAnalyticsWorkspaceCommand>
{
    public DeleteLogAnalyticsWorkspaceCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}
