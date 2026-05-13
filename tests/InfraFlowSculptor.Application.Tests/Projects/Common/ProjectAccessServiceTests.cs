using FluentAssertions;
using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Common;

public sealed class ProjectAccessServiceTests
{
    private readonly ICurrentUser _currentUser;
    private readonly IProjectRepository _projectRepository;
    private readonly ProjectAccessService _sut;

    public ProjectAccessServiceTests()
    {
        _projectRepository = Substitute.For<IProjectRepository>();
        _currentUser = Substitute.For<ICurrentUser>();
        _sut = new ProjectAccessService(_projectRepository, _currentUser);
    }

    [Fact]
    public async Task Given_ExistingProjectMember_When_VerifyReadAccessAsync_Then_UsesReadOnlyProjectLookup_Async()
    {
        // Arrange
        var projectId = ProjectId.CreateUnique();
        var userId = UserId.CreateUnique();
        var project = Project.Create(new Name("alpha"), "primary workload", userId);

        _currentUser.GetUserIdAsync(Arg.Any<CancellationToken>())
            .Returns(userId);
        _projectRepository.GetByIdWithMembersReadOnlyAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(project);

        // Act
        var result = await _sut.VerifyReadAccessAsync(projectId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(project);
        await _projectRepository.Received(1)
            .GetByIdWithMembersReadOnlyAsync(projectId, Arg.Any<CancellationToken>());
        await _projectRepository.DidNotReceive()
            .GetByIdWithMembersAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_NonMember_When_VerifyReadAccessAsync_Then_ReturnsNotFound_Async()
    {
        // Arrange
        var projectId = ProjectId.CreateUnique();
        var ownerId = UserId.CreateUnique();
        var currentUserId = UserId.CreateUnique();
        var project = Project.Create(new Name("alpha"), "primary workload", ownerId);

        _currentUser.GetUserIdAsync(Arg.Any<CancellationToken>())
            .Returns(currentUserId);
        _projectRepository.GetByIdWithMembersReadOnlyAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(project);

        // Act
        var result = await _sut.VerifyReadAccessAsync(projectId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_ReaderMember_When_VerifyWriteAccessAsync_Then_ReturnsForbidden_Async()
    {
        // Arrange
        var projectId = ProjectId.CreateUnique();
        var ownerId = UserId.CreateUnique();
        var readerId = UserId.CreateUnique();
        var project = Project.Create(new Name("alpha"), "primary workload", ownerId);
        project.AddMember(readerId, new Role(Role.RoleEnum.Reader));

        _currentUser.GetUserIdAsync(Arg.Any<CancellationToken>())
            .Returns(readerId);
        _projectRepository.GetByIdWithMembersAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(project);

        // Act
        var result = await _sut.VerifyWriteAccessAsync(projectId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
    }
}