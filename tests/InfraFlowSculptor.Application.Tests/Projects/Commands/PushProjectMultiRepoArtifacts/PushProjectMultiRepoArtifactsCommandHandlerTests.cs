using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Commands.PushProjectMultiRepoArtifacts;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.PushProjectMultiRepoArtifacts;

public sealed class PushProjectMultiRepoArtifactsCommandHandlerTests
{
    private readonly IProjectAccessService _accessService;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMultiRepoArtifactsPushService _pushService;
    private readonly Project _project;
    private readonly PushProjectMultiRepoArtifactsCommand _command;
    private readonly PushProjectMultiRepoArtifactsCommandHandler _sut;

    public PushProjectMultiRepoArtifactsCommandHandlerTests()
    {
        _accessService = Substitute.For<IProjectAccessService>();
        _projectRepository = Substitute.For<IProjectRepository>();
        _pushService = Substitute.For<IProjectMultiRepoArtifactsPushService>();
        _project = CreateProject(LayoutPresetEnum.MultiRepo);
        _command = CreateCommand(_project.Id);

        _sut = new PushProjectMultiRepoArtifactsCommandHandler(
            _accessService,
            _projectRepository,
            _pushService);
    }

    [Fact]
    public async Task Given_WriteAccessDenied_When_Handle_Then_ReturnsAccessErrorAsync()
    {
        // Arrange
        var accessError = Error.Forbidden("Forbidden", "Access denied.");
        _accessService.VerifyWriteAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(accessError);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(accessError);
        await _projectRepository.DidNotReceive()
            .GetByIdWithAllAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>());
        await _pushService.DidNotReceive()
            .PushAsync(Arg.Any<PushProjectMultiRepoArtifactsCommand>(), Arg.Any<Project>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ProjectMissingFromRepository_When_Handle_Then_ReturnsProjectNotFoundAsync()
    {
        // Arrange
        _accessService.VerifyWriteAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns((Project?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.Project.NotFoundError(_project.Id).Code);
        await _pushService.DidNotReceive()
            .PushAsync(Arg.Any<PushProjectMultiRepoArtifactsCommand>(), Arg.Any<Project>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_NonMultiRepoLayout_When_Handle_Then_RejectsTheProjectPushAsync()
    {
        // Arrange
        var splitProject = CreateProject(LayoutPresetEnum.SplitInfraCode);
        var command = CreateCommand(splitProject.Id);
        _accessService.VerifyWriteAccessAsync(splitProject.Id, Arg.Any<CancellationToken>())
            .Returns(splitProject);
        _projectRepository.GetByIdWithAllAsync(splitProject.Id, Arg.Any<CancellationToken>())
            .Returns(splitProject);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.GitRouting.LayoutNotSupportedForProjectMultiRepoPush.Code);
        await _pushService.DidNotReceive()
            .PushAsync(Arg.Any<PushProjectMultiRepoArtifactsCommand>(), Arg.Any<Project>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_MultiRepoProject_When_Handle_Then_DelegatesCommandAndProjectToNewPushServiceAsync()
    {
        // Arrange
        var expected = new PushProjectMultiRepoArtifactsResult([]);
        _accessService.VerifyWriteAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _pushService.PushAsync(_command, _project, Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(expected);
        await _pushService.Received(1)
            .PushAsync(_command, _project, Arg.Any<CancellationToken>());
    }

    private static PushProjectMultiRepoArtifactsCommand CreateCommand(ProjectId projectId)
    {
        return new PushProjectMultiRepoArtifactsCommand(
            projectId,
            [
                new InfrastructureConfigPushTarget(
                    InfrastructureConfigId.CreateUnique(),
                    [
                        new ConfigRepositoryPushTarget(
                            InfraConfigRepositoryId.CreateUnique(),
                            "feature/generated-artifacts",
                            "Update generated artifacts")
                    ])
            ]);
    }

    private static Project CreateProject(LayoutPresetEnum layoutPreset)
    {
        var project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        var layoutResult = project.SetLayoutPreset(new LayoutPreset(layoutPreset));
        if (layoutResult.IsError)
            throw new InvalidOperationException(layoutResult.FirstError.Description);

        return project;
    }
}