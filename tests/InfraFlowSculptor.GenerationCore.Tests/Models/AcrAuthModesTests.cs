using FluentAssertions;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.GenerationCore.Tests.Models;

public sealed class AcrAuthModesTests
{
    [Fact]
    public void ManagedIdentity_ShouldBeManagedIdentity()
    {
        AcrAuthModes.ManagedIdentity.Should().Be("ManagedIdentity");
    }

    [Fact]
    public void AdminCredentials_ShouldBeAdminCredentials()
    {
        AcrAuthModes.AdminCredentials.Should().Be("AdminCredentials");
    }

    [Fact]
    public void All_ShouldContainManagedIdentity()
    {
        AcrAuthModes.All.Should().Contain("ManagedIdentity");
    }

    [Fact]
    public void All_ShouldContainAdminCredentials()
    {
        AcrAuthModes.All.Should().Contain("AdminCredentials");
    }

    [Fact]
    public void All_ShouldHaveCount2()
    {
        AcrAuthModes.All.Should().HaveCount(2);
    }

    [Theory]
    [InlineData("managedidentity")]
    [InlineData("MANAGEDIDENTITY")]
    [InlineData("ManagedIdentity")]
    [InlineData("admincredentials")]
    [InlineData("ADMINCREDENTIALS")]
    [InlineData("AdminCredentials")]
    public void All_ShouldBeCaseInsensitive(string value)
    {
        AcrAuthModes.All.Contains(value).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Unknown")]
    [InlineData("ServicePrincipal")]
    public void All_GivenInvalidMode_ShouldNotContain(string value)
    {
        AcrAuthModes.All.Contains(value).Should().BeFalse();
    }
}
