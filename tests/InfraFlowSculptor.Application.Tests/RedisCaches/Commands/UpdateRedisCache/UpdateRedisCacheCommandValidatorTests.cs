using FluentAssertions;
using InfraFlowSculptor.Application.RedisCaches.Commands.UpdateRedisCache;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.RedisCaches.Commands.UpdateRedisCache;

public sealed class UpdateRedisCacheCommandValidatorTests
{
    private readonly UpdateRedisCacheCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new UpdateRedisCacheCommand(
            AzureResourceId.CreateUnique(),
            new Name("my-redis"),
            new Location(Location.LocationEnum.WestEurope),
            RedisVersion: 6,
            EnableNonSslPort: false,
            MinimumTlsVersion: "1.2",
            DisableAccessKeyAuthentication: false,
            EnableAadAuth: false);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_FailsOnName()
    {
        // Arrange
        var command = new UpdateRedisCacheCommand(
            AzureResourceId.CreateUnique(),
            null!,
            new Location(Location.LocationEnum.WestEurope),
            RedisVersion: 6,
            EnableNonSslPort: false,
            MinimumTlsVersion: null,
            DisableAccessKeyAuthentication: false,
            EnableAadAuth: false);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateRedisCacheCommand.Name));
    }

    [Fact]
    public void Given_EmptyId_When_Validate_Then_FailsOnId()
    {
        // Arrange
        var command = new UpdateRedisCacheCommand(
            null!,
            new Name("my-redis"),
            new Location(Location.LocationEnum.WestEurope),
            RedisVersion: 6,
            EnableNonSslPort: false,
            MinimumTlsVersion: null,
            DisableAccessKeyAuthentication: false,
            EnableAadAuth: false);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateRedisCacheCommand.Id));
    }

    [Fact]
    public void Given_DisableAccessKeyWithoutAadAuth_When_Validate_Then_FailsOnEnableAadAuth()
    {
        // Arrange
        var command = new UpdateRedisCacheCommand(
            AzureResourceId.CreateUnique(),
            new Name("my-redis"),
            new Location(Location.LocationEnum.WestEurope),
            RedisVersion: 6,
            EnableNonSslPort: false,
            MinimumTlsVersion: null,
            DisableAccessKeyAuthentication: true,
            EnableAadAuth: false);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateRedisCacheCommand.EnableAadAuth));
    }

    [Fact]
    public void Given_DisableAccessKeyWithAadAuth_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new UpdateRedisCacheCommand(
            AzureResourceId.CreateUnique(),
            new Name("my-redis"),
            new Location(Location.LocationEnum.WestEurope),
            RedisVersion: 6,
            EnableNonSslPort: false,
            MinimumTlsVersion: null,
            DisableAccessKeyAuthentication: true,
            EnableAadAuth: true);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
