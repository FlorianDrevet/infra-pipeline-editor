using FluentAssertions;
using InfraFlowSculptor.Application.VirtualNetworks.Commands.DeleteVirtualNetwork;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.VirtualNetworks.Commands.DeleteVirtualNetwork;

public sealed class DeleteVirtualNetworkCommandValidatorTests
{
    private readonly DeleteVirtualNetworkCommandValidator _sut = new();

    private static DeleteVirtualNetworkCommand ValidCommand() => new(
        AzureResourceId.CreateUnique());

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var result = _sut.Validate(ValidCommand());

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyId_When_Validate_Then_FailsOnId()
    {
        var command = ValidCommand() with { Id = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DeleteVirtualNetworkCommand.Id));
    }
}
