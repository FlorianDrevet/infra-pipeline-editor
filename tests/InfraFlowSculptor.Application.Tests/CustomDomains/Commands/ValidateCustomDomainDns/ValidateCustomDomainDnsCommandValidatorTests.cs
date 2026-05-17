using FluentAssertions;
using InfraFlowSculptor.Application.CustomDomains.Commands.ValidateCustomDomainDns;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.CustomDomains.Commands.ValidateCustomDomainDns;

public sealed class ValidateCustomDomainDnsCommandValidatorTests
{
    private readonly ValidateCustomDomainDnsCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new ValidateCustomDomainDnsCommand(
            AzureResourceId.CreateUnique(),
            CustomDomainId.CreateUnique());

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
        var command = new ValidateCustomDomainDnsCommand(
            null!,
            CustomDomainId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ValidateCustomDomainDnsCommand.ResourceId));
    }

    [Fact]
    public void Given_NullCustomDomainId_When_Validate_Then_FailsOnCustomDomainId()
    {
        // Arrange
        var command = new ValidateCustomDomainDnsCommand(
            AzureResourceId.CreateUnique(),
            null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ValidateCustomDomainDnsCommand.CustomDomainId));
    }
}
