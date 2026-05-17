using FluentAssertions;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.GenerationCore.Tests;

public sealed class AzureResourceDefaultsTests
{
    [Fact]
    public void MinimumTlsVersion_ShouldBe1Point2()
    {
        AzureResourceDefaults.MinimumTlsVersion.Should().Be("1.2");
    }

    [Fact]
    public void SqlServerVersion_ShouldBe12Point0()
    {
        AzureResourceDefaults.SqlServerVersion.Should().Be("12.0");
    }

    [Fact]
    public void SqlServerAdministratorLogin_ShouldBeSquladmin()
    {
        AzureResourceDefaults.SqlServerAdministratorLogin.Should().Be("sqladmin");
    }

    [Fact]
    public void StorageAccountKind_ShouldBeStorageV2()
    {
        AzureResourceDefaults.StorageAccountKind.Should().Be("StorageV2");
    }

    [Fact]
    public void StorageAccountAccessTier_ShouldBeHot()
    {
        AzureResourceDefaults.StorageAccountAccessTier.Should().Be("Hot");
    }

    [Fact]
    public void MinimumTlsVersionLabel_ShouldBeTLS1Underscore2()
    {
        AzureResourceDefaults.MinimumTlsVersionLabel.Should().Be("TLS1_2");
    }

    [Fact]
    public void AppServiceDeploymentMode_ShouldBeZip()
    {
        AzureResourceDefaults.AppServiceDeploymentMode.Should().Be("Zip");
    }

    [Fact]
    public void SqlDatabaseCollation_ShouldBeExpectedValue()
    {
        AzureResourceDefaults.SqlDatabaseCollation.Should().Be("SQL_Latin1_General_CP1_CI_AS");
    }
}
