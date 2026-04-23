using FluentValidation;

namespace InfraFlowSculptor.Application.EventHubNamespaces.Commands.DeleteEventHubNamespace;

/// <summary>Validates the <see cref="DeleteEventHubNamespaceCommand"/>.</summary>
public sealed class DeleteEventHubNamespaceCommandValidator : AbstractValidator<DeleteEventHubNamespaceCommand>
{
    public DeleteEventHubNamespaceCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}
