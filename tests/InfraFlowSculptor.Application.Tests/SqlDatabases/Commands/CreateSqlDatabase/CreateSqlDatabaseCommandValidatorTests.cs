using FluentAssertions;
using InfraFlowSculptor.Application.SqlDatabases.Commands.CreateSqlDatabase;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.SqlDatabases.Commands.CreateSqlDatabase;

public sealed class CreateSqlDatabaseCommandValidatorTests
{
    private readonly CreateSqlDatabaseCommandValidator _sut = new();

    private static CreateSqlDatabaseCommand ValidCommand() => new(
        ResourceGroupId.CreateUnique(),
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
    public void Given_NullName_When_Validate_Then_FailsOnName()
    {
        var command = ValidCommand() with { Name = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateSqlDatabaseCommand.Name));
    }

    [Fact]
    public void Given_NullResourceGroupId_When_Validate_Then_FailsOnResourceGroupId()
    {
        var command = ValidCommand() with { ResourceGroupId = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateSqlDatabaseCommand.ResourceGroupId));
    }

    [Fact]
    public void Given_EmptySqlServerId_When_Validate_Then_FailsOnSqlServerId()
    {
        var command = ValidCommand() with { SqlServerId = Guid.Empty };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateSqlDatabaseCommand.SqlServerId));
    }

    [Fact]
    public void Given_EmptyCollation_When_Validate_Then_FailsOnCollation()
    {
        var command = ValidCommand() with { Collation = "" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateSqlDatabaseCommand.Collation));
    }

    [Fact]
    public void Given_CollationTooLong_When_Validate_Then_FailsOnCollation()
    {
        var command = ValidCommand() with { Collation = new string('a', 129) };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateSqlDatabaseCommand.Collation));
    }
}
