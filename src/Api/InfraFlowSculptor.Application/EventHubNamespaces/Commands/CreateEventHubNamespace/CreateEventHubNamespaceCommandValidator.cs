using FluentValidation;

namespace InfraFlowSculptor.Application.EventHubNamespaces.Commands.CreateEventHubNamespace;

/// <summary>Validates the <see cref="CreateEventHubNamespaceCommand"/> before it is handled.</summary>
public sealed class CreateEventHubNamespaceCommandValidator : AbstractValidator<CreateEventHubNamespaceCommand>
{
    /// <summary>Initializes validation rules for creating an Event Hub Namespace.</summary>
    public CreateEventHubNamespaceCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.ResourceGroupId)
            .NotEmpty().WithMessage("ResourceGroupId is required.");
    }
}
