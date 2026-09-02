using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Common.Services;
using InfraFlowSculptor.Application.Projects.Commands.PushProjectArtifactsToMultiRepo;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.PushProjectArtifactsToMultiRepo;

public sealed class MultiRepoProjectArtifactsPushServiceTests
{
    private const string PersonalAccessToken = "pat-token";

    private readonly IGitProviderFactory _gitProviderFactory;
    private readonly IKeyVaultSecretClient _keyVaultSecretClient;
    private readonly IBlobService _blobService;
    private readonly IRepositoryTargetResolver _targetResolver;
    private readonly IGitProviderService _gitProvider;
    private readonly IGitMultiScopePushProviderService _multiScopeGitProvider;
    private readonly Project _project;
    private readonly ResolvedRepositoryTarget _infraTarget;
    private readonly ResolvedRepositoryTarget _appTarget;
    private readonly PushProjectArtifactsToMultiRepoCommand _command;
    private readonly MultiRepoProjectArtifactsPushService _sut;

    public MultiRepoProjectArtifactsPushServiceTests()
    {
        _gitProviderFactory = Substitute.For<IGitProviderFactory>();
        _keyVaultSecretClient = Substitute.For<IKeyVaultSecretClient>();
        _blobService = Substitute.For<IBlobService>();
        _targetResolver = Substitute.For<IRepositoryTargetResolver>();
        _gitProvider = Substitute.For<IGitProviderService, IGitMultiScopePushProviderService>();
        _multiScopeGitProvider = (IGitMultiScopePushProviderService)_gitProvider;

        _project = CreateConfiguredSplitProject();
        var infraRepository = _project.Repositories.Single(repository => repository.ContentKinds.Has(RepositoryContentKindsEnum.Infrastructure));
        var appRepository = _project.Repositories.Single(repository => repository.ContentKinds.Has(RepositoryContentKindsEnum.ApplicationCode));
        _infraTarget = new ResolvedRepositoryTarget(
            RepositoryId: infraRepository.Id.Value.ToString(),
            ProviderType: new GitProviderType(GitProviderTypeEnum.GitHub),
            RepositoryUrl: "https://github.com/octo-org/retail-platform-infra",
            Owner: "octo-org",
            RepositoryName: "retail-platform-infra",
            Branch: "main",
            BasePath: "infra-root",
            PipelineBasePath: "pipelines",
            PatSecretName: ProjectGitSecretNames.GetRepositoryPatSecretName(infraRepository.Id));
        _appTarget = new ResolvedRepositoryTarget(
            RepositoryId: appRepository.Id.Value.ToString(),
            ProviderType: new GitProviderType(GitProviderTypeEnum.GitHub),
            RepositoryUrl: "https://github.com/octo-org/retail-platform-app",
            Owner: "octo-org",
            RepositoryName: "retail-platform-app",
            Branch: "main",
            BasePath: string.Empty,
            PipelineBasePath: "app-pipelines",
            PatSecretName: ProjectGitSecretNames.GetRepositoryPatSecretName(appRepository.Id));
        _command = new PushProjectArtifactsToMultiRepoCommand(
            _project.Id,
            Infra: new RepoPushTarget(
                RepositoryId: infraRepository.Id,
                BranchName: "feature/generated-infra",
                CommitMessage: "Update infra artifacts"),
            Code: new RepoPushTarget(
                RepositoryId: appRepository.Id,
                BranchName: "feature/generated-code",
                CommitMessage: "Update app artifacts"));

        _sut = new MultiRepoProjectArtifactsPushService(
            new MultiScopeGitPushExecutor(_gitProviderFactory),
            _keyVaultSecretClient,
            _blobService,
            _targetResolver);
    }

    [Fact]
    public async Task Given_ProviderWithoutMultiScopeSupport_When_PushAsync_Then_ReturnsPerRepoFailuresAsync()
    {
        // Arrange
        var unsupportedProvider = Substitute.For<IGitProviderService>();
        var expectedError = Errors.GitRepository.PushFailed(
            "The selected Git provider does not support multi-scope pushes.");

        ConfigureSuccessfulPrerequisites();
        ConfigureGeneratedArtifacts();
        _gitProviderFactory.Create(Arg.Any<GitProviderType>()).Returns(unsupportedProvider);

        // Act
        var result = await _sut.PushAsync(_command, _project, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Results.Should().SatisfyRespectively(
            infraResult =>
            {
                infraResult.RepositoryId.Should().Be(_command.Infra!.RepositoryId);
                infraResult.Success.Should().BeFalse();
                infraResult.ErrorCode.Should().Be(expectedError.Code);
                infraResult.ErrorDescription.Should().Be(expectedError.Description);
            },
            codeResult =>
            {
                codeResult.RepositoryId.Should().Be(_command.Code!.RepositoryId);
                codeResult.Success.Should().BeFalse();
                codeResult.ErrorCode.Should().Be(expectedError.Code);
                codeResult.ErrorDescription.Should().Be(expectedError.Description);
            });
    }

    [Fact]
    public async Task Given_InfraPushFailsAndCodePushSucceeds_When_PushAsync_Then_ReturnsIndependentRepoResultsAsync()
    {
        // Arrange
        var infraPushError = Errors.GitRepository.PushFailed("infra push failed");

        ConfigureSuccessfulPrerequisites();
        ConfigureGeneratedArtifacts();
        _gitProviderFactory.Create(Arg.Any<GitProviderType>()).Returns(_gitProvider);
        _multiScopeGitProvider.PushScopedFilesAsync(Arg.Any<MultiScopeGitPushRequest>(), Arg.Any<CancellationToken>())
            .Returns(
                infraPushError,
                new PushBicepToGitResult(
                    BranchName: "feature/generated-code",
                    BranchUrl: "https://github.com/octo-org/retail-platform-app/tree/feature/generated-code",
                    CommitSha: "def456",
                    FileCount: 2));

        // Act
        var result = await _sut.PushAsync(_command, _project, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Results.Should().SatisfyRespectively(
            infraResult =>
            {
                infraResult.RepositoryId.Should().Be(_command.Infra!.RepositoryId);
                infraResult.Success.Should().BeFalse();
                infraResult.ErrorCode.Should().Be(infraPushError.Code);
                infraResult.ErrorDescription.Should().Be(infraPushError.Description);
                infraResult.FileCount.Should().Be(0);
            },
            codeResult =>
            {
                codeResult.RepositoryId.Should().Be(_command.Code!.RepositoryId);
                codeResult.Success.Should().BeTrue();
                codeResult.BranchUrl.Should().Be("https://github.com/octo-org/retail-platform-app/tree/feature/generated-code");
                codeResult.CommitSha.Should().Be("def456");
                codeResult.FileCount.Should().Be(2);
                codeResult.ErrorCode.Should().BeNull();
                codeResult.ErrorDescription.Should().BeNull();
            });
    }

    [Fact]
    public async Task Given_InfraPushIsCanceled_When_PushAsync_Then_PropagatesCancellationAndDoesNotPushCodeRepositoryAsync()
    {
        // Arrange
        ConfigureSuccessfulPrerequisites();
        ConfigureGeneratedArtifacts();
        _gitProviderFactory.Create(Arg.Any<GitProviderType>()).Returns(_gitProvider);
        _multiScopeGitProvider.PushScopedFilesAsync(
                Arg.Any<MultiScopeGitPushRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ErrorOr<PushBicepToGitResult>>(
                new OperationCanceledException()));

        // Act
        Func<Task> act = async () => await _sut.PushAsync(_command, _project, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        await _multiScopeGitProvider.Received(1).PushScopedFilesAsync(
            Arg.Any<MultiScopeGitPushRequest>(),
            CancellationToken.None);
    }

    private void ConfigureSuccessfulPrerequisites()
    {
        _targetResolver.Resolve(_project, config: null, ArtifactKind.Pipeline)
            .Returns(_infraTarget);
        _targetResolver.Resolve(_project, config: null, ArtifactKind.ApplicationPipeline)
            .Returns(_appTarget);
        _keyVaultSecretClient.GetSecretAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(PersonalAccessToken);
    }

    private void ConfigureGeneratedArtifacts()
    {
        ConfigureBlobListing(
            $"bicep/project/{_project.Id.Value}/",
            [
                ("20260512094500/main.bicep", "main-bicep"),
            ]);
        ConfigureBlobListing(
            $"pipeline/project/{_project.Id.Value}/",
            [
                ("20260512094500/infra/.azuredevops/infra-ci.yml", "infra-ci"),
                ("20260512094500/app/apps/api/api/ci.yml", "app-ci"),
            ]);
        ConfigureBlobListing(
            $"bootstrap/project/{_project.Id.Value}/",
            [
                ("20260512094500/infra/bootstrap.yml", "infra-bootstrap"),
                ("20260512094500/app/bootstrap.yml", "app-bootstrap"),
            ]);
    }

    private void ConfigureBlobListing(
        string prefix,
        IReadOnlyCollection<(string RelativePath, string Content)> blobs)
    {
        _blobService.ListBlobsAsync(prefix)
            .Returns(blobs.Select(blob => $"{prefix}{blob.RelativePath}").ToList());

        foreach (var (relativePath, content) in blobs)
        {
            _blobService.DownloadContentAsync($"{prefix}{relativePath}")
                .Returns(content);
        }
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
