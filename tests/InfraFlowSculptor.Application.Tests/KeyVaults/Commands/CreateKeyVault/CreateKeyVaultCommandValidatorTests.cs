using FluentAssertions;
using InfraFlowSculptor.Application.KeyVaults.Commands.CreateKeyVault;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.KeyVaults.Commands.CreateKeyVault;

public sealed class CreateKeyVaultCommandValidatorTests
{
    private readonly CreateKeyVaultCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var command = new CreateKeyVaultCommand(
            ResourceGroupId.CreateUnique(),
            new Name("my-keyvault"),
            new Location(Location.LocationEnum.WestEurope));

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyResourceGroupId_When_Validate_Then_Fails()
    {
        var command = new CreateKeyVaultCommand(
            null!,
            new Name("my-keyvault"),
            new Location(Location.LocationEnum.WestEurope));

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateKeyVaultCommand.ResourceGroupId));
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_Fails()
    {
        var command = new CreateKeyVaultCommand(
            ResourceGroupId.CreateUnique(),
            null!,
            new Location(Location.LocationEnum.WestEurope));

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateKeyVaultCommand.Name));
    }

    [Fact]
    public void Given_EmptyLocation_When_Validate_Then_Fails()
    {
        var command = new CreateKeyVaultCommand(
            ResourceGroupId.CreateUnique(),
            new Name("my-keyvault"),
            null!);

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateKeyVaultCommand.Location));
    }
}
