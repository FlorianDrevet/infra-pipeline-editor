using FluentValidation;

namespace InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.CreateServiceBusNamespace;

/// <summary>
/// Validates the <see cref="CreateServiceBusNamespaceCommand"/> before it is handled.
/// </summary>
public sealed class CreateServiceBusNamespaceCommandValidator : AbstractValidator<CreateServiceBusNamespaceCommand>
{
    /// <summary>Initializes validation rules for creating a Service Bus Namespace.</summary>
    public CreateServiceBusNamespaceCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.ResourceGroupId)
            .NotEmpty().WithMessage("ResourceGroupId is required.");
    }
}
