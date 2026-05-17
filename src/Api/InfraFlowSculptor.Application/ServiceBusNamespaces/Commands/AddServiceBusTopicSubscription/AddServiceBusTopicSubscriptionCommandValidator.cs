using FluentValidation;

namespace InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.AddServiceBusTopicSubscription;

/// <summary>Validates the <see cref="AddServiceBusTopicSubscriptionCommand"/> before it is handled.</summary>
public sealed class AddServiceBusTopicSubscriptionCommandValidator : AbstractValidator<AddServiceBusTopicSubscriptionCommand>
{
    /// <summary>Initializes validation rules for adding a Service Bus topic subscription.</summary>
    public AddServiceBusTopicSubscriptionCommandValidator()
    {
        RuleFor(x => x.ServiceBusNamespaceId)
            .NotEmpty().WithMessage("ServiceBusNamespaceId is required.");

        RuleFor(x => x.TopicName)
            .NotEmpty().WithMessage("TopicName is required.");

        RuleFor(x => x.SubscriptionName)
            .NotEmpty().WithMessage("SubscriptionName is required.");
    }
}
