using FluentValidation;

namespace InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.UpdateServiceBusNamespace;

/// <summary>
/// Validates the <see cref="UpdateServiceBusNamespaceCommand"/> before it is handled.
/// </summary>
public sealed class UpdateServiceBusNamespaceCommandValidator : AbstractValidator<UpdateServiceBusNamespaceCommand>
{
    /// <summary>Initializes validation rules for updating a Service Bus Namespace.</summary>
    public UpdateServiceBusNamespaceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");
    }
}
