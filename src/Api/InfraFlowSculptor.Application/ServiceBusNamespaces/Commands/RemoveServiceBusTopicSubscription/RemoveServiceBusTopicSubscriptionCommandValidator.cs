using FluentValidation;

namespace InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.RemoveServiceBusTopicSubscription;

/// <summary>Validates the <see cref="RemoveServiceBusTopicSubscriptionCommand"/> before it is handled.</summary>
public sealed class RemoveServiceBusTopicSubscriptionCommandValidator : AbstractValidator<RemoveServiceBusTopicSubscriptionCommand>
{
    /// <summary>Initializes validation rules for removing a Service Bus topic subscription.</summary>
    public RemoveServiceBusTopicSubscriptionCommandValidator()
    {
        RuleFor(x => x.ServiceBusNamespaceId)
            .NotEmpty().WithMessage("ServiceBusNamespaceId is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("SubscriptionId is required.");
    }
}
