using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectMember;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.RemoveProjectMember;

public sealed class RemoveProjectMemberCommandHandlerTests
{
    private readonly IProjectAccessService _accessService;
    private readonly IProjectRepository _projectRepository;
    private readonly Project _project;
    private readonly UserId _ownerId;
    private readonly RemoveProjectMemberCommandHandler _sut;

    public RemoveProjectMemberCommandHandlerTests()
    {
        _accessService = Substitute.For<IProjectAccessService>();
        _projectRepository = Substitute.For<IProjectRepository>();
        _ownerId = UserId.CreateUnique();
        _project = Project.Create(new Name("test-project"), "Test project", _ownerId);
        _sut = new RemoveProjectMemberCommandHandler(_accessService, _projectRepository);
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        var command = new RemoveProjectMemberCommand(_project.Id, Guid.NewGuid());
        _accessService.VerifyOwnerAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(Error.Unauthorized());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Given_MemberNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange — user ID does not match any member
        var command = new RemoveProjectMemberCommand(_project.Id, Guid.NewGuid());
        _accessService.VerifyOwnerAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_TargetIsOwner_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange — try to remove the owner
        var command = new RemoveProjectMemberCommand(_project.Id, _ownerId.Value);
        _accessService.VerifyOwnerAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Given_ValidRequest_When_Handle_Then_ReturnsDeletedAsync()
    {
        // Arrange — add a non-owner member, then remove them
        var memberId = UserId.CreateUnique();
        _project.AddMember(memberId, new Role(Role.RoleEnum.Contributor));
        var command = new RemoveProjectMemberCommand(_project.Id, memberId.Value);

        _accessService.VerifyOwnerAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        _projectRepository.Received(1).Update(Arg.Any<Project>());
    }
}
