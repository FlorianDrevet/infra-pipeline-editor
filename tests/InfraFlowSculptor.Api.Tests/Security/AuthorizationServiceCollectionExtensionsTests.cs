using FluentAssertions;
using InfraFlowSculptor.Api.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InfraFlowSculptor.Api.Tests.Security;

public sealed class AuthorizationServiceCollectionExtensionsTests
{
    [Fact]
    public async Task Given_ApiAuthorizationIsRegistered_When_ResolvingIsAdminPolicy_Then_ItRequiresTheAdminRoleAsync()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApiAuthorization();

        await using var serviceProvider = services.BuildServiceProvider();
        var authorizationPolicyProvider = serviceProvider.GetRequiredService<IAuthorizationPolicyProvider>();
        var authorizationPolicy = await authorizationPolicyProvider.GetPolicyAsync(AuthorizationPolicyNames.IsAdmin);
        var rolesRequirement = authorizationPolicy?.Requirements.OfType<RolesAuthorizationRequirement>().SingleOrDefault();

        // Assert
        authorizationPolicy.Should().NotBeNull();
        rolesRequirement.Should().NotBeNull();
        rolesRequirement!.AllowedRoles.Should().Contain(AuthorizationPolicyNames.AdminRole);
    }
}