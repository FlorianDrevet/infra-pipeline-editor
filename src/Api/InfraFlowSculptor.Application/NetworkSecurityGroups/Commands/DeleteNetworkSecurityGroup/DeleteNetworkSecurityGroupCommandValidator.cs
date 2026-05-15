using FluentValidation;

namespace InfraFlowSculptor.Application.NetworkSecurityGroups.Commands.DeleteNetworkSecurityGroup;

/// <summary>Validates the <see cref="DeleteNetworkSecurityGroupCommand"/>.</summary>
public sealed class DeleteNetworkSecurityGroupCommandValidator : AbstractValidator<DeleteNetworkSecurityGroupCommand>
{
    public DeleteNetworkSecurityGroupCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}
