using FluentAssertions;
using InfraFlowSculptor.Api.Common;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Web;
using NSubstitute;
using System.Security.Claims;
using Xunit;

namespace InfraFlowSculptor.Api.Tests.Security;

public sealed class UserProvisioningMiddlewareTests
{
    [Fact]
    public async Task Given_AuthenticatedRequest_When_InvokeAsync_Then_StoresProvisionedUserIdAsync()
    {
        // Arrange
        var provisionedUserId = UserId.CreateUnique();
        var userProvisioningService = Substitute.For<IUserProvisioningService>();
        userProvisioningService
            .EnsureProvisionedAsync(Arg.Any<EntraId>(), Arg.Any<Name>(), Arg.Any<CancellationToken>())
            .Returns(provisionedUserId);

        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(
            new ClaimsIdentity(
            [
                new Claim(ClaimConstants.ObjectId, Guid.NewGuid().ToString()),
                new Claim(ClaimConstants.Name, "Ada Lovelace")
            ],
            authenticationType: "Test"));

        var nextInvoked = false;
        var sut = new UserProvisioningMiddleware(_ =>
        {
            nextInvoked = true;
            return Task.CompletedTask;
        });

        // Act
        await sut.InvokeAsync(context, userProvisioningService);

        // Assert
        context.Items[UserProvisioningMiddleware.UserIdItemKey].Should().Be(provisionedUserId);
        nextInvoked.Should().BeTrue();
        await userProvisioningService.Received(1)
            .EnsureProvisionedAsync(
                Arg.Any<EntraId>(),
                Arg.Is<Name>(name => name.FirstName == "Ada" && name.LastName == "Lovelace"),
                context.RequestAborted);
    }

    [Fact]
    public void Given_UserProvisioningMiddleware_When_InspectingInvokeAsyncSignature_Then_DependsOnApplicationServiceInsteadOfDbContext()
    {
        // Arrange
        var provisioningServiceType = Type.GetType(
            "InfraFlowSculptor.Application.Common.Interfaces.Services.IUserProvisioningService, InfraFlowSculptor.Application");
        var invokeAsyncMethod = typeof(UserProvisioningMiddleware).GetMethod(nameof(UserProvisioningMiddleware.InvokeAsync));

        // Act
        var parameterTypes = invokeAsyncMethod?.GetParameters().Select(parameter => parameter.ParameterType).ToArray();

        // Assert
        provisioningServiceType.Should().NotBeNull();
        invokeAsyncMethod.Should().NotBeNull();
        parameterTypes.Should().Contain(provisioningServiceType!);
        parameterTypes.Should().NotContain(typeof(ProjectDbContext));
    }
}