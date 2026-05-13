using FluentValidation;

namespace InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.RemoveServiceBusQueue;

/// <summary>Validates the <see cref="RemoveServiceBusQueueCommand"/> before it is handled.</summary>
public sealed class RemoveServiceBusQueueCommandValidator : AbstractValidator<RemoveServiceBusQueueCommand>
{
    /// <summary>Initializes validation rules for removing a Service Bus queue.</summary>
    public RemoveServiceBusQueueCommandValidator()
    {
        RuleFor(x => x.ServiceBusNamespaceId)
            .NotEmpty().WithMessage("ServiceBusNamespaceId is required.");

        RuleFor(x => x.QueueId)
            .NotEmpty().WithMessage("QueueId is required.");
    }
}