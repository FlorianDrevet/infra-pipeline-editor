using FluentAssertions;
using InfraFlowSculptor.Application.SqlServers.Commands.UpdateSqlServer;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.SqlServers.Commands.UpdateSqlServer;

public sealed class UpdateSqlServerCommandValidatorTests
{
    private readonly UpdateSqlServerCommandValidator _sut = new();

    private static UpdateSqlServerCommand ValidCommand() => new(
        AzureResourceId.CreateUnique(),
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
    public void Given_EmptyId_When_Validate_Then_FailsOnId()
    {
        var command = ValidCommand() with { Id = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateSqlServerCommand.Id));
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_FailsOnName()
    {
        var command = ValidCommand() with { Name = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateSqlServerCommand.Name));
    }

    [Fact]
    public void Given_EmptyLocation_When_Validate_Then_FailsOnLocation()
    {
        var command = ValidCommand() with { Location = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateSqlServerCommand.Location));
    }

    [Fact]
    public void Given_EmptyVersion_When_Validate_Then_FailsOnVersion()
    {
        var command = ValidCommand() with { Version = "" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateSqlServerCommand.Version));
    }

    [Fact]
    public void Given_EmptyAdministratorLogin_When_Validate_Then_FailsOnAdministratorLogin()
    {
        var command = ValidCommand() with { AdministratorLogin = "" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateSqlServerCommand.AdministratorLogin));
    }

    [Fact]
    public void Given_AdministratorLoginTooLong_When_Validate_Then_FailsOnAdministratorLogin()
    {
        var command = ValidCommand() with { AdministratorLogin = new string('a', 129) };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateSqlServerCommand.AdministratorLogin));
    }
}
