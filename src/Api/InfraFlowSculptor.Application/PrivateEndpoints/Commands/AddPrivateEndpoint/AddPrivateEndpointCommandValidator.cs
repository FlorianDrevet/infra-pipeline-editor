using FluentValidation;

namespace InfraFlowSculptor.Application.PrivateEndpoints.Commands.AddPrivateEndpoint;

/// <summary>Validates the <see cref="AddPrivateEndpointCommand"/> before it is handled.</summary>
public sealed class AddPrivateEndpointCommandValidator : AbstractValidator<AddPrivateEndpointCommand>
{
    /// <summary>Initializes validation rules for adding a private endpoint.</summary>
    public AddPrivateEndpointCommandValidator()
    {
        RuleFor(x => x.ResourceId)
            .NotNull().WithMessage("ResourceId is required.");

        RuleFor(x => x.SubnetId)
            .NotNull().WithMessage("SubnetId is required.");

        RuleFor(x => x.GroupId)
            .NotEmpty().WithMessage("GroupId is required.")
            .MaximumLength(100).WithMessage("GroupId must not exceed 100 characters.");

        RuleFor(x => x.CustomNetworkInterfaceName)
            .MaximumLength(80).WithMessage("CustomNetworkInterfaceName must not exceed 80 characters.")
            .When(x => x.CustomNetworkInterfaceName is not null);
    }
}
