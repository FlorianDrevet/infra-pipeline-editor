using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Commands.CreateProject;
using InfraFlowSculptor.Domain.ProjectAggregate;
using NSubstitute;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.CreateProject;

public sealed class CreateProjectCommandUnauthorizedTests
{
    [Fact]
    public async Task Given_CurrentUserNotProvisioned_When_Handle_Then_ReturnsUnauthorizedAsync()
    {
        // Arrange
        var repository = Substitute.For<IProjectRepository>();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.GetUserIdAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<InfraFlowSculptor.Domain.UserAggregate.ValueObjects.UserId>(
                new UnauthorizedAccessException("User was not provisioned.")));
        var sut = new CreateProjectCommandHandler(repository, currentUser);

        // Act
        var result = await sut.Handle(new CreateProjectCommand("RetailApi", "desc"), CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        await repository.DidNotReceive().AddAsync(Arg.Any<Project>());
    }
}