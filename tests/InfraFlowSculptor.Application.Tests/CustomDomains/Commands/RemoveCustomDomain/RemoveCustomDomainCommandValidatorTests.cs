using FluentAssertions;
using InfraFlowSculptor.Application.CustomDomains.Commands.RemoveCustomDomain;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.CustomDomains.Commands.RemoveCustomDomain;

public sealed class RemoveCustomDomainCommandValidatorTests
{
    private readonly RemoveCustomDomainCommandValidator _sut = new();

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
    public void Given_EmptyResourceId_When_Validate_Then_FailsOnResourceId()
    {
        // Arrange
        var command = CreateCommand() with { ResourceId = null! };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemoveCustomDomainCommand.ResourceId));
    }

    [Fact]
    public void Given_EmptyCustomDomainId_When_Validate_Then_FailsOnCustomDomainId()
    {
        // Arrange
        var command = CreateCommand() with { CustomDomainId = null! };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemoveCustomDomainCommand.CustomDomainId));
    }

    private static RemoveCustomDomainCommand CreateCommand()
    {
        return new RemoveCustomDomainCommand(
            AzureResourceId.CreateUnique(),
            CustomDomainId.CreateUnique());
    }
}
