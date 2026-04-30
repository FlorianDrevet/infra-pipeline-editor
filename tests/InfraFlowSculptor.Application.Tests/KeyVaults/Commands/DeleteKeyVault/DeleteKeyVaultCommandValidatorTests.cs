using FluentAssertions;
using InfraFlowSculptor.Application.KeyVaults.Commands.DeleteKeyVault;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.KeyVaults.Commands.DeleteKeyVault;

public sealed class DeleteKeyVaultCommandValidatorTests
{
    private const string IdProperty = nameof(DeleteKeyVaultCommand.Id);

    private readonly DeleteKeyVaultCommandValidator _sut = new();

    [Fact]
    public void Given_NonNullId_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new DeleteKeyVaultCommand(AzureResourceId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_NullId_When_Validate_Then_FailsOnId()
    {
        // Arrange
        var command = new DeleteKeyVaultCommand(null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == IdProperty);
    }
}
