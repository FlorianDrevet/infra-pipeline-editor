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
    private const string PersonalAccessToken = "pat-token";

    private readonly IProjectAccessService _accessService;
    private readonly IProjectRepository _projectRepository;
    private readonly IKeyVaultSecretClient _keyVaultSecretClient;
    private readonly IMultiRepoProjectArtifactsPushService _pushService;
    private readonly Project _project;
    private readonly PushProjectArtifactsToMultiRepoCommand _command;
    private readonly PushProjectArtifactsToMultiRepoCommandHandler _sut;

    public PushProjectArtifactsToMultiRepoCommandHandlerTests()
    {
        _accessService = Substitute.For<IProjectAccessService>();
        _projectRepository = Substitute.For<IProjectRepository>();
        _keyVaultSecretClient = Substitute.For<IKeyVaultSecretClient>();
        _pushService = Substitute.For<IMultiRepoProjectArtifactsPushService>();

        _project = CreateConfiguredSplitProject();
        _command = new PushProjectArtifactsToMultiRepoCommand(
            _project.Id,
            Infra: new RepoPushTarget(
                Alias: "infra",
                BranchName: "feature/generated-infra",
                CommitMessage: "Update infra artifacts"),
            Code: new RepoPushTarget(
                Alias: "code",
                BranchName: "feature/generated-code",
                CommitMessage: "Update app artifacts"));

        _sut = new PushProjectArtifactsToMultiRepoCommandHandler(
            _accessService,
            _projectRepository,
            _keyVaultSecretClient,
            _pushService);
    }

    [Fact]
    public async Task Given_ValidSplitProject_When_Handle_Then_DelegatesToPushServiceAsync()
    {
        // Arrange
        var expected = new PushProjectArtifactsToMultiRepoResult(
        [
            new RepoPushResult("infra", true, "https://example/infra", "abc123", 3, null, null),
            new RepoPushResult("code", true, "https://example/code", "def456", 2, null, null),
        ]);

        _accessService.VerifyWriteAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _keyVaultSecretClient.GetSecretAsync($"git-pat-{_project.Id.Value}", Arg.Any<CancellationToken>())
            .Returns(PersonalAccessToken);
        _pushService.PushAsync(_command, _project, PersonalAccessToken, Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(expected);

        await _pushService.Received(1)
            .PushAsync(_command, _project, PersonalAccessToken, Arg.Any<CancellationToken>());
    }

    private static Project CreateConfiguredSplitProject()
    {
        var project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        var layoutResult = project.SetLayoutPreset(new LayoutPreset(LayoutPresetEnum.SplitInfraCode));
        if (layoutResult.IsError)
            throw new InvalidOperationException(layoutResult.FirstError.Description);

        var infraAlias = RepositoryAlias.Create("infra");
        if (infraAlias.IsError)
            throw new InvalidOperationException(infraAlias.FirstError.Description);

        var codeAlias = RepositoryAlias.Create("code");
        if (codeAlias.IsError)
            throw new InvalidOperationException(codeAlias.FirstError.Description);

        var infraKinds = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure);
        if (infraKinds.IsError)
            throw new InvalidOperationException(infraKinds.FirstError.Description);

        var codeKinds = RepositoryContentKinds.Create(RepositoryContentKindsEnum.ApplicationCode);
        if (codeKinds.IsError)
            throw new InvalidOperationException(codeKinds.FirstError.Description);

        var infraRepositoryResult = project.AddRepository(
            infraAlias.Value,
            new GitProviderType(GitProviderTypeEnum.GitHub),
            "https://github.com/octo-org/retail-platform-infra",
            "main",
            infraKinds.Value);
        if (infraRepositoryResult.IsError)
            throw new InvalidOperationException(infraRepositoryResult.FirstError.Description);

        var codeRepositoryResult = project.AddRepository(
            codeAlias.Value,
            new GitProviderType(GitProviderTypeEnum.GitHub),
            "https://github.com/octo-org/retail-platform-app",
            "main",
            codeKinds.Value);
        if (codeRepositoryResult.IsError)
            throw new InvalidOperationException(codeRepositoryResult.FirstError.Description);

        return project;
    }
}
