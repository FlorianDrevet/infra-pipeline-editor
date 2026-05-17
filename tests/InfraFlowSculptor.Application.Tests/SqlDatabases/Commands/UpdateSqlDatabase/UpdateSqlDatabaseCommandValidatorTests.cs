using FluentAssertions;
using InfraFlowSculptor.Application.SqlDatabases.Commands.UpdateSqlDatabase;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.SqlDatabases.Commands.UpdateSqlDatabase;

public sealed class UpdateSqlDatabaseCommandValidatorTests
{
    private readonly UpdateSqlDatabaseCommandValidator _sut = new();

    private static UpdateSqlDatabaseCommand ValidCommand() => new(
        AzureResourceId.CreateUnique(),
        new Name("sqldb-test"),
        new Location(Location.LocationEnum.WestEurope),
        SqlServerId: Guid.NewGuid(),
        Collation: "SQL_Latin1_General_CP1_CI_AS");

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var result = _sut.Validate(ValidCommand());

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyId_When_Validate_Then_FailsOnId()
    {
        var command = ValidCommand() with { Id = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateSqlDatabaseCommand.Id));
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_FailsOnName()
    {
        var command = ValidCommand() with { Name = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateSqlDatabaseCommand.Name));
    }

    [Fact]
    public void Given_EmptyLocation_When_Validate_Then_FailsOnLocation()
    {
        var command = ValidCommand() with { Location = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateSqlDatabaseCommand.Location));
    }

    [Fact]
    public void Given_EmptySqlServerId_When_Validate_Then_FailsOnSqlServerId()
    {
        var command = ValidCommand() with { SqlServerId = Guid.Empty };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateSqlDatabaseCommand.SqlServerId));
    }

    [Fact]
    public void Given_EmptyCollation_When_Validate_Then_FailsOnCollation()
    {
        var command = ValidCommand() with { Collation = "" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateSqlDatabaseCommand.Collation));
    }
}
