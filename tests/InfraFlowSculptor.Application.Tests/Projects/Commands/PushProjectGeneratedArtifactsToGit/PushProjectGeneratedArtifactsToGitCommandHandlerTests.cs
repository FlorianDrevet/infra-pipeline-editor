using FluentAssertions;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Common.Services;
using InfraFlowSculptor.Application.Projects.Commands.PushProjectGeneratedArtifactsToGit;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.PushProjectGeneratedArtifactsToGit;

public sealed class PushProjectGeneratedArtifactsToGitCommandHandlerTests
{
    private readonly IProjectAccessService _accessService;
    private readonly IProjectRepository _projectRepository;
    private readonly IKeyVaultSecretClient _keyVaultSecretClient;
    private readonly IGitProviderFactory _gitProviderFactory;
    private readonly IBlobService _blobService;
    private readonly IRepositoryTargetResolver _targetResolver;
    private readonly IGitProviderService _gitProvider;
    private readonly IGitMultiScopePushProviderService _multiScopeGitProvider;
    private readonly Project _project;
    private readonly ResolvedRepositoryTarget _target;
    private readonly PushProjectGeneratedArtifactsToGitCommand _command;
    private readonly PushProjectGeneratedArtifactsToGitCommandHandler _sut;

    public PushProjectGeneratedArtifactsToGitCommandHandlerTests()
    {
        _accessService = Substitute.For<IProjectAccessService>();
        _projectRepository = Substitute.For<IProjectRepository>();
        _keyVaultSecretClient = Substitute.For<IKeyVaultSecretClient>();
        _gitProviderFactory = Substitute.For<IGitProviderFactory>();
        _blobService = Substitute.For<IBlobService>();
        _targetResolver = Substitute.For<IRepositoryTargetResolver>();
        _gitProvider = Substitute.For<IGitProviderService, IGitMultiScopePushProviderService>();
        _multiScopeGitProvider = (IGitMultiScopePushProviderService)_gitProvider;

        _project = CreateConfiguredProject();
        _target = new ResolvedRepositoryTarget(
            Alias: "default",
            ProviderType: new GitProviderType(GitProviderTypeEnum.GitHub),
            RepositoryUrl: "https://github.com/octo-org/retail-platform",
            Owner: "octo-org",
            RepositoryName: "retail-platform",
            Branch: "main",
            BasePath: "infra-root",
            PipelineBasePath: "pipelines",
            PatSecretName: null);
        _command = new PushProjectGeneratedArtifactsToGitCommand(
            _project.Id,
            BranchName: "feature/generated-update",
            CommitMessage: "Update generated artifacts");

        _sut = new PushProjectGeneratedArtifactsToGitCommandHandler(
            _accessService,
            _projectRepository,
            _keyVaultSecretClient,
            new MultiScopeGitPushExecutor(_gitProviderFactory),
            _blobService,
            _targetResolver);
    }

    [Fact]
    public async Task Given_LatestArtifactsAcrossAllScopes_When_Handle_Then_PushesNormalizedScopedFilesAsync()
    {
        // Arrange
        MultiScopeGitPushRequest? capturedRequest = null;

        _accessService.VerifyWriteAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _targetResolver.Resolve(_project, config: null, ArtifactKind.Pipeline)
            .Returns(_target);
        _keyVaultSecretClient.GetSecretAsync($"git-pat-{_project.Id.Value}", Arg.Any<CancellationToken>())
            .Returns("pat-token");
        _gitProviderFactory.Create(_target.ProviderType).Returns(_gitProvider);

        ConfigureLatestBlobListing(
            "bicep",
            [
                ("20260512094500/main.bicep", "main-bicep"),
                ("20260512094500/modules/app.bicep", "module-bicep"),
                ("20260511120000/old/main.bicep", "old-bicep"),
            ]);
        ConfigureLatestBlobListing(
            "pipeline",
            [
                ("20260512094500/app/apps/api/api/ci.yml", "app-ci"),
                ("20260512094500/infra/.azuredevops/platform.yml", "infra-platform"),
                ("20260511120000/.azuredevops/legacy.yml", "old-pipeline"),
            ]);
        ConfigureLatestBlobListing(
            "bootstrap",
            [
                ("20260512094500/bootstrap.yml", "bootstrap-pipeline"),
                ("20260511120000/old/bootstrap.yml", "old-bootstrap"),
            ]);

        _multiScopeGitProvider.PushScopedFilesAsync(
                Arg.Do<MultiScopeGitPushRequest>(request => capturedRequest = request),
                Arg.Any<CancellationToken>())
            .Returns(new PushBicepToGitResult(
                BranchName: _command.BranchName,
                BranchUrl: "https://github.com/octo-org/retail-platform/tree/feature/generated-update",
                CommitSha: "abc123",
                FileCount: 5));

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.FileCount.Should().Be(5);
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Scopes.Should().HaveCount(2);

        capturedRequest.Scopes[0].BasePath.Should().Be("infra-root");
        capturedRequest.Scopes[0].Files.Should().BeEquivalentTo(new Dictionary<string, string>
        {
            ["main.bicep"] = "main-bicep",
            ["modules/app.bicep"] = "module-bicep",
        });

        capturedRequest.Scopes[1].BasePath.Should().Be("pipelines");
        capturedRequest.Scopes[1].Files.Should().BeEquivalentTo(new Dictionary<string, string>
        {
            ["apps/api/ci.yml"] = "app-ci",
            [".azuredevops/platform.yml"] = "infra-platform",
            ["bootstrap.yml"] = "bootstrap-pipeline",
        });
    }

    [Fact]
    public async Task Given_PipelineArtifactsMissing_When_Handle_Then_ReturnsPipelineNotFoundWithoutCreatingGitProviderAsync()
    {
        // Arrange
        _accessService.VerifyWriteAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _targetResolver.Resolve(_project, config: null, ArtifactKind.Pipeline)
            .Returns(_target);
        _keyVaultSecretClient.GetSecretAsync($"git-pat-{_project.Id.Value}", Arg.Any<CancellationToken>())
            .Returns("pat-token");

        ConfigureLatestBlobListing(
            "bicep",
            [
                ("20260512094500/main.bicep", "main-bicep"),
            ]);
        _blobService.ListBlobsAsync($"pipeline/project/{_project.Id.Value}/")
            .Returns([]);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.Project.PipelineFilesNotFoundError(_project.Id.Value).Code);
        _gitProviderFactory.DidNotReceive().Create(Arg.Any<GitProviderType>());
        await _multiScopeGitProvider.DidNotReceive()
            .PushScopedFilesAsync(Arg.Any<MultiScopeGitPushRequest>(), Arg.Any<CancellationToken>());
    }

    private void ConfigureLatestBlobListing(
        string artifactType,
        IReadOnlyCollection<(string RelativePath, string Content)> blobs)
    {
        var prefix = $"{artifactType}/project/{_project.Id.Value}/";
        _blobService.ListBlobsAsync(prefix)
            .Returns(blobs.Select(blob => $"{prefix}{blob.RelativePath}").ToList());

        foreach (var (relativePath, content) in blobs)
        {
            _blobService.DownloadContentAsync($"{prefix}{relativePath}")
                .Returns(content);
        }
    }

    private static Project CreateConfiguredProject()
    {
        var project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        var layoutResult = project.SetLayoutPreset(new LayoutPreset(LayoutPresetEnum.AllInOne));
        if (layoutResult.IsError)
            throw new InvalidOperationException(layoutResult.FirstError.Description);

        var alias = RepositoryAlias.Create("default");
        if (alias.IsError)
            throw new InvalidOperationException(alias.FirstError.Description);

        var contentKinds = RepositoryContentKinds.Create(
            RepositoryContentKindsEnum.Infrastructure | RepositoryContentKindsEnum.ApplicationCode);
        if (contentKinds.IsError)
            throw new InvalidOperationException(contentKinds.FirstError.Description);

        var repositoryResult = project.AddRepository(
            alias.Value,
            new GitProviderType(GitProviderTypeEnum.GitHub),
            "https://github.com/octo-org/retail-platform",
            "main",
            contentKinds.Value);
        if (repositoryResult.IsError)
            throw new InvalidOperationException(repositoryResult.FirstError.Description);

        return project;
    }
}
