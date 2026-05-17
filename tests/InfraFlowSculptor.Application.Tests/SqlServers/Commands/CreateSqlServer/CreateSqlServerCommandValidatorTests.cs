using FluentAssertions;
using InfraFlowSculptor.Application.SqlServers.Commands.CreateSqlServer;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.SqlServers.Commands.CreateSqlServer;

public sealed class CreateSqlServerCommandValidatorTests
{
    private readonly CreateSqlServerCommandValidator _sut = new();

    private static CreateSqlServerCommand ValidCommand() => new(
        ResourceGroupId.CreateUnique(),
        new Name("sql-server-test"),
        new Location(Location.LocationEnum.WestEurope),
        Version: "12.0",
        AdministratorLogin: "sqladmin");

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
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateSqlServerCommand.Name));
    }

    [Fact]
    public void Given_NullResourceGroupId_When_Validate_Then_FailsOnResourceGroupId()
    {
        var command = ValidCommand() with { ResourceGroupId = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateSqlServerCommand.ResourceGroupId));
    }

    [Fact]
    public void Given_EmptyVersion_When_Validate_Then_FailsOnVersion()
    {
        var command = ValidCommand() with { Version = "" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateSqlServerCommand.Version));
    }

    [Fact]
    public void Given_EmptyAdministratorLogin_When_Validate_Then_FailsOnAdministratorLogin()
    {
        var command = ValidCommand() with { AdministratorLogin = "" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateSqlServerCommand.AdministratorLogin));
    }

    [Fact]
    public void Given_AdministratorLoginTooLong_When_Validate_Then_FailsOnAdministratorLogin()
    {
        var command = ValidCommand() with { AdministratorLogin = new string('a', 129) };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateSqlServerCommand.AdministratorLogin));
    }
}
