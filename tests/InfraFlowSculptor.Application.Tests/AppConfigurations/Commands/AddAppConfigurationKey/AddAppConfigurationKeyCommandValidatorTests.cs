using FluentAssertions;
using InfraFlowSculptor.Application.AppConfigurations.Commands.AddAppConfigurationKey;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.AppConfigurations.Commands.AddAppConfigurationKey;

public sealed class AddAppConfigurationKeyCommandValidatorTests
{
    private const string SecretNameProperty = nameof(AddAppConfigurationKeyCommand.SecretName);

    private readonly AddAppConfigurationKeyCommandValidator _sut = new();

    [Fact]
    public void Given_ValidKeyVaultSecretName_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = CreateCommand("jwt-secret");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("JWT_SECRET")]
    [InlineData("invalid_secret")]
    public void Given_SecretNameContainingUnderscore_When_Validate_Then_FailsOnSecretName(string secretName)
    {
        // Arrange
        var command = CreateCommand(secretName);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == SecretNameProperty);
    }

    [Fact]
    public void Given_SecretNameLongerThan127Characters_When_Validate_Then_FailsOnSecretName()
    {
        // Arrange
        var command = CreateCommand(new string('a', 128));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == SecretNameProperty);
    }

    private static AddAppConfigurationKeyCommand CreateCommand(string secretName)
    {
        return new AddAppConfigurationKeyCommand(
            new AzureResourceId(Guid.NewGuid()),
            Key: "JwtSettings:Secret",
            Label: null,
            EnvironmentValues: null,
            KeyVaultResourceId: new AzureResourceId(Guid.NewGuid()),
            SecretName: secretName);
    }
}