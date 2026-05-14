using FluentValidation;

namespace InfraFlowSculptor.Application.VirtualNetworks.Commands.DeleteVirtualNetwork;

/// <summary>Validates the <see cref="DeleteVirtualNetworkCommand"/>.</summary>
public sealed class DeleteVirtualNetworkCommandValidator : AbstractValidator<DeleteVirtualNetworkCommand>
{
    public DeleteVirtualNetworkCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}
