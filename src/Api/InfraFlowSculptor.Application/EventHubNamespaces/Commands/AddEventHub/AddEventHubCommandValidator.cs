using FluentValidation;

namespace InfraFlowSculptor.Application.EventHubNamespaces.Commands.AddEventHub;

/// <summary>Validates the <see cref="AddEventHubCommand"/> before it is handled.</summary>
public sealed class AddEventHubCommandValidator : AbstractValidator<AddEventHubCommand>
{
    /// <summary>Initializes validation rules for adding an event hub.</summary>
    public AddEventHubCommandValidator()
    {
        RuleFor(x => x.EventHubNamespaceId)
            .NotEmpty().WithMessage("EventHubNamespaceId is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");
    }
}