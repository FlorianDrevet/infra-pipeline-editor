using FluentAssertions;
using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Services;

public sealed class UserProvisioningServiceTests
{
    [Fact]
    public void Given_UserProvisioningAbstraction_When_InspectingInfrastructureAssembly_Then_ImplementationExists()
    {
        // Arrange
        var abstractionType = Type.GetType(
            "InfraFlowSculptor.Application.Common.Interfaces.Services.IUserProvisioningService, InfraFlowSculptor.Application");
        var implementationType = Type.GetType(
            "InfraFlowSculptor.Infrastructure.Services.UserProvisioningService, InfraFlowSculptor.Infrastructure");

        // Assert
        abstractionType.Should().NotBeNull();
        implementationType.Should().NotBeNull();
        abstractionType!.IsAssignableFrom(implementationType!).Should().BeTrue();
    }

    [Fact]
    public void Given_ProvisionUserSql_When_ReadingCommandText_Then_ItDoesNotContainBackslashEscapedIdentifiers()
    {
        // Arrange
        var implementationType = Type.GetType(
            "InfraFlowSculptor.Infrastructure.Services.UserProvisioningService, InfraFlowSculptor.Infrastructure");

        // Act
        var provisionUserSql = implementationType?
            .GetField("ProvisionUserSql", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?
            .GetRawConstantValue() as string;

        // Assert
        implementationType.Should().NotBeNull();
        provisionUserSql.Should().NotBeNullOrWhiteSpace();
        provisionUserSql.Should().Contain("\"User\"");
        provisionUserSql.Should().NotContain("\\\"");
    }
}