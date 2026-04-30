using FluentAssertions;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.SqlServerAggregate;
using InfraFlowSculptor.Domain.SqlServerAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.SqlServerAggregate;

public sealed class SqlServerTests
{
    private const string DefaultServerName = "sql-prod-data";
    private const string DefaultAdministratorLogin = "sqladmin";
    private const Location.LocationEnum DefaultLocationValue = Location.LocationEnum.WestEurope;

    private static SqlServer CreateValidSqlServer(bool isExisting = false)
    {
        return SqlServer.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultServerName),
            new Location(DefaultLocationValue),
            new SqlServerVersion(SqlServerVersion.SqlServerVersionEnum.V12),
            DefaultAdministratorLogin,
            isExisting: isExisting);
    }

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Act
        var sut = CreateValidSqlServer();

        // Assert
        sut.Id.Should().NotBeNull();
        sut.Name.Value.Should().Be(DefaultServerName);
        sut.Location.Value.Should().Be(DefaultLocationValue);
        sut.Version.Value.Should().Be(SqlServerVersion.SqlServerVersionEnum.V12);
        sut.AdministratorLogin.Should().Be(DefaultAdministratorLogin);
        sut.IsExisting.Should().BeFalse();
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_NotExisting_When_Update_Then_UpdatesAllProperties()
    {
        // Arrange
        var sut = CreateValidSqlServer();
        var newName = new Name("sql-renamed");
        var newLocation = new Location(Location.LocationEnum.EastUS);
        const string newAdmin = "newadmin";

        // Act
        sut.Update(newName, newLocation, new SqlServerVersion(SqlServerVersion.SqlServerVersionEnum.V12), newAdmin);

        // Assert
        sut.Name.Should().Be(newName);
        sut.Location.Should().Be(newLocation);
        sut.AdministratorLogin.Should().Be(newAdmin);
    }

    [Fact]
    public void Given_IsExistingResource_When_Update_Then_OnlyNameAndLocationChange()
    {
        // Arrange
        var sut = CreateValidSqlServer(isExisting: true);
        var newName = new Name("sql-renamed");
        var newLocation = new Location(Location.LocationEnum.EastUS);

        // Act
        sut.Update(newName, newLocation, new SqlServerVersion(SqlServerVersion.SqlServerVersionEnum.V12), "newadmin");

        // Assert
        sut.Name.Should().Be(newName);
        sut.Location.Should().Be(newLocation);
        sut.AdministratorLogin.Should().Be(DefaultAdministratorLogin);
    }

    [Fact]
    public void Given_NewEnvironment_When_SetEnvironmentSettings_Then_AddsEntry()
    {
        // Arrange
        var sut = CreateValidSqlServer();

        // Act
        sut.SetEnvironmentSettings("prod", "1.2");

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        sut.EnvironmentSettings.Single().MinimalTlsVersion.Should().Be("1.2");
    }

    [Fact]
    public void Given_ExistingEnvironment_When_SetEnvironmentSettings_Then_UpdatesEntryInPlace()
    {
        // Arrange
        var sut = CreateValidSqlServer();
        sut.SetEnvironmentSettings("prod", "1.0");

        // Act
        sut.SetEnvironmentSettings("prod", "1.2");

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        sut.EnvironmentSettings.Single().MinimalTlsVersion.Should().Be("1.2");
    }

    [Fact]
    public void Given_IsExistingResource_When_SetEnvironmentSettings_Then_DoesNothing()
    {
        // Arrange
        var sut = CreateValidSqlServer(isExisting: true);

        // Act
        sut.SetEnvironmentSettings("prod", "1.2");

        // Assert
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_MultipleEnvironments_When_SetAllEnvironmentSettings_Then_ReplacesAll()
    {
        // Arrange
        var sut = CreateValidSqlServer();
        sut.SetEnvironmentSettings("dev", "1.0");
        var settings = new[]
        {
            ("staging", (string?)"1.2"),
            ("prod", (string?)"1.2"),
        };

        // Act
        sut.SetAllEnvironmentSettings(settings);

        // Assert
        sut.EnvironmentSettings.Should().HaveCount(2);
        sut.EnvironmentSettings.Should().NotContain(es => es.EnvironmentName == "dev");
    }
}
