using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Commands.UpdateProjectEnvironment;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;
using Tag = InfraFlowSculptor.Domain.UserAggregate.ValueObjects.Tag;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.UpdateProjectEnvironment;

public sealed class UpdateProjectEnvironmentCommandHandlerTests
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly UpdateProjectEnvironmentCommandHandler _sut;

    public UpdateProjectEnvironmentCommandHandlerTests()
    {
        _projectRepository = Substitute.For<IProjectRepository>();
        _accessService = Substitute.For<IProjectAccessService>();
        _mapper = Substitute.For<IMapper>();
        _sut = new UpdateProjectEnvironmentCommandHandler(_projectRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        var projectId = ProjectId.CreateUnique();
        var envId = ProjectEnvironmentDefinitionId.CreateUnique();
        var command = CreateCommand(projectId, envId);
        _accessService.VerifyWriteAccessAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(Error.Forbidden("Forbidden", "Forbidden"));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Given_ProjectNotFound_When_Handle_Then_ReturnsNotFoundErrorAsync()
    {
        // Arrange
        var userId = UserId.CreateUnique();
        var project = Project.Create(new Name("TestProject"), null, userId);
        var projectId = project.Id;
        var envId = ProjectEnvironmentDefinitionId.CreateUnique();
        var command = CreateCommand(projectId, envId);

        _accessService.VerifyWriteAccessAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(project);
        _projectRepository.GetByIdWithAllAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((Project?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_ValidCommand_When_Handle_Then_UpdatesEnvironmentAndReturnsResultAsync()
    {
        // Arrange
        var userId = UserId.CreateUnique();
        var project = Project.Create(new Name("TestProject"), null, userId);
        var envData = new EnvironmentDefinitionData(
            new Name("Dev"),
            new ShortName("dev"),
            new Prefix("d"),
            new Suffix("-dev"),
            new Location(Location.LocationEnum.WestEurope),
            new SubscriptionId(Guid.NewGuid()),
            new Order(1),
            new RequiresApproval(false),
            null,
            Enumerable.Empty<Tag>());
        var env = project.AddEnvironment(envData);
        var command = CreateCommand(project.Id, env.Id);

        _accessService.VerifyWriteAccessAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);
        _projectRepository.GetByIdWithAllAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);

        var expectedResult = new ProjectEnvironmentDefinitionResult(
            env.Id, new Name("UpdatedName"), "upd", "u", "-upd", "WestEurope",
            Guid.NewGuid(), 1, false, null, []);
        _mapper.Map<ProjectEnvironmentDefinitionResult>(Arg.Any<Domain.ProjectAggregate.Entities.ProjectEnvironmentDefinition>())
            .Returns(expectedResult);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _projectRepository.Received(1).Update(project);
    }

    private static UpdateProjectEnvironmentCommand CreateCommand(
        ProjectId projectId,
        ProjectEnvironmentDefinitionId envId)
    {
        return new UpdateProjectEnvironmentCommand(
            projectId, envId,
            "UpdatedName", "upd", "u", "-upd", "WestEurope",
            Guid.NewGuid(), 1, false, null, []);
    }
}
