using FluentAssertions;
using InfraFlowSculptor.Application.StorageAccounts.Commands.CreateStorageAccount;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.StorageAccounts.Commands.CreateStorageAccount;

public sealed class CreateStorageAccountCommandValidatorTests
{
    private const string KindProperty = nameof(CreateStorageAccountCommand.Kind);
    private const string AccessTierProperty = nameof(CreateStorageAccountCommand.AccessTier);
    private const string MinimumTlsVersionProperty = nameof(CreateStorageAccountCommand.MinimumTlsVersion);

    private readonly CreateStorageAccountCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = CreateCommand();

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_InvalidKind_When_Validate_Then_FailsOnKind()
    {
        // Arrange
        var command = CreateCommand(kind: "Archive");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == KindProperty);
    }

    [Fact]
    public void Given_InvalidAccessTier_When_Validate_Then_FailsOnAccessTier()
    {
        // Arrange
        var command = CreateCommand(accessTier: "Cold");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == AccessTierProperty);
    }

    [Fact]
    public void Given_InvalidMinimumTlsVersion_When_Validate_Then_FailsOnMinimumTlsVersion()
    {
        // Arrange
        var command = CreateCommand(minimumTlsVersion: "Tls13");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == MinimumTlsVersionProperty);
    }

    private static CreateStorageAccountCommand CreateCommand(
        string kind = nameof(StorageAccountKind.Kind.StorageV2),
        string accessTier = nameof(StorageAccessTier.Tier.Hot),
        string minimumTlsVersion = nameof(StorageAccountTlsVersion.Version.Tls12))
    {
        return new CreateStorageAccountCommand(
            ResourceGroupId.CreateUnique(),
            new Name("storageacct"),
            new Location(Location.LocationEnum.WestEurope),
            kind,
            accessTier,
            AllowBlobPublicAccess: false,
            EnableHttpsTrafficOnly: true,
            minimumTlsVersion);
    }
}
