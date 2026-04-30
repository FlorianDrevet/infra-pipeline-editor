using FluentAssertions;
using InfraFlowSculptor.Application.StorageAccounts.Commands.DeleteStorageAccount;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.StorageAccounts.Commands.DeleteStorageAccount;

public sealed class DeleteStorageAccountCommandValidatorTests
{
    private const string IdProperty = nameof(DeleteStorageAccountCommand.Id);

    private readonly DeleteStorageAccountCommandValidator _sut = new();

    [Fact]
    public void Given_NonNullId_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new DeleteStorageAccountCommand(AzureResourceId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_NullId_When_Validate_Then_FailsOnId()
    {
        // Arrange
        var command = new DeleteStorageAccountCommand(null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == IdProperty);
    }
}
