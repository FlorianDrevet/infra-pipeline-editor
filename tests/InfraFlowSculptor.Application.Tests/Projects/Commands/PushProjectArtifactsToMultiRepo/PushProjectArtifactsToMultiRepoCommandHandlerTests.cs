using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Commands.PushProjectArtifactsToMultiRepo;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.PushProjectArtifactsToMultiRepo;

public sealed class PushProjectArtifactsToMultiRepoCommandHandlerTests
{
    private readonly IProjectAccessService _accessService;
    private readonly IProjectRepository _projectRepository;
    private readonly IMultiRepoProjectArtifactsPushService _pushService;
    private readonly Project _project;
    private readonly PushProjectArtifactsToMultiRepoCommand _command;
    private readonly PushProjectArtifactsToMultiRepoCommandHandler _sut;

    public PushProjectArtifactsToMultiRepoCommandHandlerTests()
    {
        _accessService = Substitute.For<IProjectAccessService>();
        _projectRepository = Substitute.For<IProjectRepository>();
        _pushService = Substitute.For<IMultiRepoProjectArtifactsPushService>();

        _project = CreateConfiguredSplitProject();
        var infraRepository = _project.Repositories.Single(repository => repository.ContentKinds.Has(RepositoryContentKindsEnum.Infrastructure));
        var codeRepository = _project.Repositories.Single(repository => repository.ContentKinds.Has(RepositoryContentKindsEnum.ApplicationCode));
        _command = new PushProjectArtifactsToMultiRepoCommand(
            _project.Id,
            Infra: new RepoPushTarget(
                RepositoryId: infraRepository.Id,
                BranchName: "feature/generated-infra",
                CommitMessage: "Update infra artifacts"),
            Code: new RepoPushTarget(
                RepositoryId: codeRepository.Id,
                BranchName: "feature/generated-code",
                CommitMessage: "Update app artifacts"));

        _sut = new PushProjectArtifactsToMultiRepoCommandHandler(
            _accessService,
            _projectRepository,
            _pushService);
    }

    [Fact]
    public async Task Given_ValidSplitProject_When_Handle_Then_DelegatesToPushServiceAsync()
    {
        // Arrange
        var expected = new PushProjectArtifactsToMultiRepoResult(
        [
            new RepoPushResult(_command.Infra!.RepositoryId, true, "https://example/infra", "abc123", 3, null, null),
            new RepoPushResult(_command.Code!.RepositoryId, true, "https://example/code", "def456", 2, null, null),
        ]);

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

    private static Project CreateConfiguredSplitProject()
    {
        var project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        var layoutResult = project.SetLayoutPreset(new LayoutPreset(LayoutPresetEnum.SplitInfraCode));
        if (layoutResult.IsError)
            throw new InvalidOperationException(layoutResult.FirstError.Description);

        var infraKinds = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure);
        if (infraKinds.IsError)
            throw new InvalidOperationException(infraKinds.FirstError.Description);

        var codeKinds = RepositoryContentKinds.Create(RepositoryContentKindsEnum.ApplicationCode);
        if (codeKinds.IsError)
            throw new InvalidOperationException(codeKinds.FirstError.Description);

        var infraRepositoryResult = project.AddRepository(
            new GitProviderType(GitProviderTypeEnum.GitHub),
            "https://github.com/octo-org/retail-platform-infra",
            "main",
            infraKinds.Value);
        if (infraRepositoryResult.IsError)
            throw new InvalidOperationException(infraRepositoryResult.FirstError.Description);

        var codeRepositoryResult = project.AddRepository(
            new GitProviderType(GitProviderTypeEnum.GitHub),
            "https://github.com/octo-org/retail-platform-app",
            "main",
            codeKinds.Value);
        if (codeRepositoryResult.IsError)
            throw new InvalidOperationException(codeRepositoryResult.FirstError.Description);

        return project;
    }
}
