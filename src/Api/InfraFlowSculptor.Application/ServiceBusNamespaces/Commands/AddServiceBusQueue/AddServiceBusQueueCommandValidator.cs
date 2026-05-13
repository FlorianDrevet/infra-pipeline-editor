using FluentValidation;

namespace InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.AddServiceBusQueue;

/// <summary>Validates the <see cref="AddServiceBusQueueCommand"/> before it is handled.</summary>
public sealed class AddServiceBusQueueCommandValidator : AbstractValidator<AddServiceBusQueueCommand>
{
    /// <summary>Initializes validation rules for adding a Service Bus queue.</summary>
    public AddServiceBusQueueCommandValidator()
    {
        RuleFor(x => x.ServiceBusNamespaceId)
            .NotEmpty().WithMessage("ServiceBusNamespaceId is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");
    }
}