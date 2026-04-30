using FluentAssertions;
using InfraFlowSculptor.Application.SqlServers.Commands.DeleteSqlServer;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.SqlServers.Commands.DeleteSqlServer;

public sealed class DeleteSqlServerCommandValidatorTests
{
    private const string IdProperty = nameof(DeleteSqlServerCommand.Id);

    private readonly DeleteSqlServerCommandValidator _sut = new();

    [Fact]
    public void Given_NonNullId_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new DeleteSqlServerCommand(AzureResourceId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_NullId_When_Validate_Then_FailsOnId()
    {
        // Arrange
        var command = new DeleteSqlServerCommand(null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == IdProperty);
    }
}
