using FluentValidation;

namespace InfraFlowSculptor.Application.EventHubNamespaces.Commands.AddEventHubConsumerGroup;

/// <summary>Validates the <see cref="AddEventHubConsumerGroupCommand"/> before it is handled.</summary>
public sealed class AddEventHubConsumerGroupCommandValidator : AbstractValidator<AddEventHubConsumerGroupCommand>
{
    /// <summary>Initializes validation rules for adding an event hub consumer group.</summary>
    public AddEventHubConsumerGroupCommandValidator()
    {
        RuleFor(x => x.EventHubNamespaceId)
            .NotEmpty().WithMessage("EventHubNamespaceId is required.");

        RuleFor(x => x.EventHubName)
            .NotEmpty().WithMessage("EventHubName is required.");

        RuleFor(x => x.ConsumerGroupName)
            .NotEmpty().WithMessage("ConsumerGroupName is required.");
    }
}
