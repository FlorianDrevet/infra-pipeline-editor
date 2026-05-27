using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Commands.AddProjectMember;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.AddProjectMember;

public sealed class AddProjectMemberCommandHandlerTests
{
    private readonly IProjectAccessService _accessService;
    private readonly IProjectRepository _projectRepository;
    private readonly IMapper _mapper;
    private readonly Project _project;
    private readonly AddProjectMemberCommandHandler _sut;

    public AddProjectMemberCommandHandlerTests()
    {
        _accessService = Substitute.For<IProjectAccessService>();
        _projectRepository = Substitute.For<IProjectRepository>();
        _mapper = Substitute.For<IMapper>();
        _project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());

        _accessService.VerifyOwnerAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.Update(Arg.Any<Project>())
            .Returns(callInfo => (Project)callInfo.Args()[0]);
        _mapper.Map<ProjectResult>(Arg.Any<Project>())
            .Returns(callInfo => CreateProjectResult((Project)callInfo.Args()[0]));

        _sut = new AddProjectMemberCommandHandler(_accessService, _projectRepository, _mapper);
    }

    [Fact]
    public async Task Given_LowercaseRole_When_Handle_Then_AddsMemberWithParsedRoleAsync()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new AddProjectMemberCommand(_project.Id, userId, "contributor");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _projectRepository.Received(1).Update(Arg.Is<Project>(project =>
            project.Members.Any(member =>
                member.UserId == UserId.Create(userId)
                && member.Role.Value == Role.RoleEnum.Contributor)));
    }

    private static ProjectResult CreateProjectResult(Project project)
    {
        return new ProjectResult(project.Id, project.Name, project.Description, [], [], null, [], [], []);
    }
}
