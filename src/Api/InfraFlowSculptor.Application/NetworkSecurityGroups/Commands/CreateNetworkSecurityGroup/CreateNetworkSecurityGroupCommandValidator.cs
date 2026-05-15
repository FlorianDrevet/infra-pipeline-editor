using FluentValidation;

namespace InfraFlowSculptor.Application.NetworkSecurityGroups.Commands.CreateNetworkSecurityGroup;

/// <summary>Validates the <see cref="CreateNetworkSecurityGroupCommand"/>.</summary>
public sealed class CreateNetworkSecurityGroupCommandValidator : AbstractValidator<CreateNetworkSecurityGroupCommand>
{
    public CreateNetworkSecurityGroupCommandValidator()
    {
        RuleFor(x => x.ResourceGroupId).NotEmpty().WithMessage("ResourceGroupId is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Location).NotEmpty().WithMessage("Location is required.");
    }
}
