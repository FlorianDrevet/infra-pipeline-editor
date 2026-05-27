using FluentAssertions;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.UpdateServiceBusNamespace;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.ServiceBusNamespaces.Commands.UpdateServiceBusNamespace;

public sealed class UpdateServiceBusNamespaceCommandValidatorTests
{
    private readonly UpdateServiceBusNamespaceCommandValidator _sut = new();

    private static UpdateServiceBusNamespaceCommand ValidCommand() => new(
        AzureResourceId.CreateUnique(),
        new Name("sb-test"),
        new Location(Location.LocationEnum.WestEurope));

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
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateServiceBusNamespaceCommand.Id));
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_FailsOnName()
    {
        var command = ValidCommand() with { Name = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateServiceBusNamespaceCommand.Name));
    }
}
