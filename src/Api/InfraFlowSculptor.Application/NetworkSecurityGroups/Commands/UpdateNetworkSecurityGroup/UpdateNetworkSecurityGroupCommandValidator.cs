using FluentValidation;

namespace InfraFlowSculptor.Application.NetworkSecurityGroups.Commands.UpdateNetworkSecurityGroup;

/// <summary>Validates the <see cref="UpdateNetworkSecurityGroupCommand"/>.</summary>
public sealed class UpdateNetworkSecurityGroupCommandValidator : AbstractValidator<UpdateNetworkSecurityGroupCommand>
{
    public UpdateNetworkSecurityGroupCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Location).NotEmpty().WithMessage("Location is required.");
    }
}
