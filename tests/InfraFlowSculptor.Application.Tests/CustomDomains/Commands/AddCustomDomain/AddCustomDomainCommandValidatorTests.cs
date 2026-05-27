using FluentAssertions;
using InfraFlowSculptor.Application.CustomDomains.Commands.AddCustomDomain;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.CustomDomains.Commands.AddCustomDomain;

public sealed class AddCustomDomainCommandValidatorTests
{
    private readonly AddCustomDomainCommandValidator _sut = new();

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
    public void Given_NullResourceId_When_Validate_Then_FailsOnResourceId()
    {
        // Arrange
        var command = CreateCommand() with { ResourceId = null! };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddCustomDomainCommand.ResourceId));
    }

    [Fact]
    public void Given_EmptyEnvironmentName_When_Validate_Then_FailsOnEnvironmentName()
    {
        // Arrange
        var command = CreateCommand(environmentName: "");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddCustomDomainCommand.EnvironmentName));
    }

    [Fact]
    public void Given_EnvironmentNameTooLong_When_Validate_Then_FailsOnEnvironmentName()
    {
        // Arrange
        var command = CreateCommand(environmentName: new string('a', 101));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddCustomDomainCommand.EnvironmentName));
    }

    [Fact]
    public void Given_EmptyDomainName_When_Validate_Then_FailsOnDomainName()
    {
        // Arrange
        var command = CreateCommand(domainName: "");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddCustomDomainCommand.DomainName));
    }

    [Fact]
    public void Given_DomainNameTooLong_When_Validate_Then_FailsOnDomainName()
    {
        // Arrange
        var command = CreateCommand(domainName: new string('a', 254));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddCustomDomainCommand.DomainName));
    }

    [Fact]
    public void Given_InvalidDomainFormat_When_Validate_Then_FailsOnDomainName()
    {
        // Arrange
        var command = CreateCommand(domainName: "not-a-domain");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddCustomDomainCommand.DomainName));
    }

    [Fact]
    public void Given_InvalidCertificateMode_When_Validate_Then_FailsOnCertificateMode()
    {
        // Arrange
        var command = CreateCommand(certificateMode: "Invalid");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddCustomDomainCommand.CertificateMode));
    }

    [Fact]
    public void Given_KeyVaultCertModeWithoutKeyVaultUrl_When_Validate_Then_FailsOnKeyVaultUrl()
    {
        // Arrange
        var command = CreateCommand(
            certificateMode: "KeyVaultCertificate",
            managedIdentityResourceId: "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/id");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddCustomDomainCommand.KeyVaultUrl));
    }

    [Fact]
    public void Given_KeyVaultCertModeWithoutManagedIdentityResourceId_When_Validate_Then_FailsOnManagedIdentityResourceId()
    {
        // Arrange
        var command = CreateCommand(
            certificateMode: "KeyVaultCertificate",
            keyVaultUrl: "https://myvault.vault.azure.net/secrets/cert/version");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddCustomDomainCommand.ManagedIdentityResourceId));
    }

    [Fact]
    public void Given_ManualCertModeWithoutCertificateName_When_Validate_Then_FailsOnCertificateName()
    {
        // Arrange
        var command = CreateCommand(certificateMode: "ManualCertificate");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddCustomDomainCommand.CertificateName));
    }

    [Fact]
    public void Given_ValidKeyVaultCertModeWithAllFields_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = CreateCommand(
            certificateMode: "KeyVaultCertificate",
            keyVaultUrl: "https://myvault.vault.azure.net/secrets/cert/version",
            managedIdentityResourceId: "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/id");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_ValidManualCertModeWithCertName_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = CreateCommand(
            certificateMode: "ManualCertificate",
            certificateName: "my-cert");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    private static AddCustomDomainCommand CreateCommand(
        string environmentName = "production",
        string domainName = "api.example.com",
        string certificateMode = "ManagedCertificate",
        string? keyVaultUrl = null,
        string? managedIdentityResourceId = null,
        string? certificateName = null)
    {
        return new AddCustomDomainCommand(
            AzureResourceId.CreateUnique(),
            environmentName,
            domainName,
            certificateMode,
            keyVaultUrl,
            managedIdentityResourceId,
            certificateName);
    }
}
