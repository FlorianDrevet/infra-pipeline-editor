using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Commands.UpdateProjectMemberRole;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.UpdateProjectMemberRole;

public sealed class UpdateProjectMemberRoleCommandHandlerTests
{
    private readonly IProjectAccessService _accessService;
    private readonly IProjectRepository _projectRepository;
    private readonly IMapper _mapper;
    private readonly Project _project;
    private readonly UserId _memberUserId;
    private readonly UpdateProjectMemberRoleCommandHandler _sut;

    public UpdateProjectMemberRoleCommandHandlerTests()
    {
        _accessService = Substitute.For<IProjectAccessService>();
        _projectRepository = Substitute.For<IProjectRepository>();
        _mapper = Substitute.For<IMapper>();
        _project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        _memberUserId = UserId.CreateUnique();
        _project.AddMember(_memberUserId, new Role(Role.RoleEnum.Reader));

        _accessService.VerifyOwnerAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.Update(Arg.Any<Project>())
            .Returns(callInfo => (Project)callInfo.Args()[0]);
        _mapper.Map<ProjectResult>(Arg.Any<Project>())
            .Returns(callInfo => CreateProjectResult((Project)callInfo.Args()[0]));

        _sut = new UpdateProjectMemberRoleCommandHandler(_accessService, _projectRepository, _mapper);
    }

    [Fact]
    public async Task Given_LowercaseRole_When_Handle_Then_UpdatesMemberRoleAsync()
    {
        // Arrange
        var command = new UpdateProjectMemberRoleCommand(_project.Id, _memberUserId.Value, "contributor");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _projectRepository.Received(1).Update(Arg.Is<Project>(project =>
            project.Members.Any(member =>
                member.UserId == _memberUserId
                && member.Role.Value == Role.RoleEnum.Contributor)));
    }

    private static ProjectResult CreateProjectResult(Project project)
    {
        return new ProjectResult(project.Id, project.Name, project.Description, [], [], null, [], [], []);
    }
}
