using FluentValidation;

namespace InfraFlowSculptor.Application.EventHubNamespaces.Commands.RemoveEventHubConsumerGroup;

/// <summary>Validates the <see cref="RemoveEventHubConsumerGroupCommand"/> before it is handled.</summary>
public sealed class RemoveEventHubConsumerGroupCommandValidator : AbstractValidator<RemoveEventHubConsumerGroupCommand>
{
    /// <summary>Initializes validation rules for removing an event hub consumer group.</summary>
    public RemoveEventHubConsumerGroupCommandValidator()
    {
        RuleFor(x => x.EventHubNamespaceId)
            .NotEmpty().WithMessage("EventHubNamespaceId is required.");

        RuleFor(x => x.ConsumerGroupId)
            .NotEmpty().WithMessage("ConsumerGroupId is required.");
    }
}