using FluentValidation;

namespace InfraFlowSculptor.Application.EventHubNamespaces.Commands.RemoveEventHub;

/// <summary>Validates the <see cref="RemoveEventHubCommand"/> before it is handled.</summary>
public sealed class RemoveEventHubCommandValidator : AbstractValidator<RemoveEventHubCommand>
{
    /// <summary>Initializes validation rules for removing an event hub.</summary>
    public RemoveEventHubCommandValidator()
    {
        RuleFor(x => x.EventHubNamespaceId)
            .NotEmpty().WithMessage("EventHubNamespaceId is required.");

        RuleFor(x => x.EventHubId)
            .NotEmpty().WithMessage("EventHubId is required.");
    }
}