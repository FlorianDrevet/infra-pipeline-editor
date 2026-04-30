using FluentAssertions;
using InfraFlowSculptor.Application.SqlDatabases.Commands.DeleteSqlDatabase;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.SqlDatabases.Commands.DeleteSqlDatabase;

public sealed class DeleteSqlDatabaseCommandValidatorTests
{
    private const string IdProperty = nameof(DeleteSqlDatabaseCommand.Id);

    private readonly DeleteSqlDatabaseCommandValidator _sut = new();

    [Fact]
    public void Given_NonNullId_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new DeleteSqlDatabaseCommand(AzureResourceId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_NullId_When_Validate_Then_FailsOnId()
    {
        // Arrange
        var command = new DeleteSqlDatabaseCommand(null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == IdProperty);
    }
}
