using FluentValidation;

namespace InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.DeleteServiceBusNamespace;

/// <summary>Validates the <see cref="DeleteServiceBusNamespaceCommand"/>.</summary>
public sealed class DeleteServiceBusNamespaceCommandValidator : AbstractValidator<DeleteServiceBusNamespaceCommand>
{
    public DeleteServiceBusNamespaceCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}
