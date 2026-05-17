using FluentAssertions;
using InfraFlowSculptor.Application.StorageAccounts.Commands.UpdateStorageAccount;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.StorageAccounts.Commands.UpdateStorageAccount;

public sealed class UpdateStorageAccountCommandValidatorTests
{
    private readonly UpdateStorageAccountCommandValidator _sut = new();

    private static UpdateStorageAccountCommand ValidCommand() => new(
        AzureResourceId.CreateUnique(),
        new Name("mystorage"),
        new Location(Location.LocationEnum.FranceCentral),
        Kind: "StorageV2",
        AccessTier: "Hot",
        AllowBlobPublicAccess: false,
        EnableHttpsTrafficOnly: true,
        MinimumTlsVersion: "Tls12");

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
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateStorageAccountCommand.Id));
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_FailsOnName()
    {
        var command = ValidCommand() with { Name = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateStorageAccountCommand.Name));
    }

    [Fact]
    public void Given_EmptyLocation_When_Validate_Then_FailsOnLocation()
    {
        var command = ValidCommand() with { Location = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateStorageAccountCommand.Location));
    }

    [Fact]
    public void Given_EmptyKind_When_Validate_Then_FailsOnKind()
    {
        var command = ValidCommand() with { Kind = "" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateStorageAccountCommand.Kind));
    }

    [Fact]
    public void Given_InvalidKind_When_Validate_Then_FailsOnKind()
    {
        var command = ValidCommand() with { Kind = "Archive" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateStorageAccountCommand.Kind));
    }

    [Fact]
    public void Given_EmptyAccessTier_When_Validate_Then_FailsOnAccessTier()
    {
        var command = ValidCommand() with { AccessTier = "" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateStorageAccountCommand.AccessTier));
    }

    [Fact]
    public void Given_InvalidAccessTier_When_Validate_Then_FailsOnAccessTier()
    {
        var command = ValidCommand() with { AccessTier = "Cold" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateStorageAccountCommand.AccessTier));
    }

    [Fact]
    public void Given_EmptyMinimumTlsVersion_When_Validate_Then_FailsOnMinimumTlsVersion()
    {
        var command = ValidCommand() with { MinimumTlsVersion = "" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateStorageAccountCommand.MinimumTlsVersion));
    }

    [Fact]
    public void Given_InvalidMinimumTlsVersion_When_Validate_Then_FailsOnMinimumTlsVersion()
    {
        var command = ValidCommand() with { MinimumTlsVersion = "Tls13" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateStorageAccountCommand.MinimumTlsVersion));
    }
}
