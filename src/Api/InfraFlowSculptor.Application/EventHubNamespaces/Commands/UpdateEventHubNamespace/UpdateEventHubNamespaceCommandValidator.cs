using FluentValidation;

namespace InfraFlowSculptor.Application.EventHubNamespaces.Commands.UpdateEventHubNamespace;

/// <summary>Validates the <see cref="UpdateEventHubNamespaceCommand"/> before it is handled.</summary>
public sealed class UpdateEventHubNamespaceCommandValidator : AbstractValidator<UpdateEventHubNamespaceCommand>
{
    /// <summary>Initializes validation rules for updating an Event Hub Namespace.</summary>
    public UpdateEventHubNamespaceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");
    }
}
