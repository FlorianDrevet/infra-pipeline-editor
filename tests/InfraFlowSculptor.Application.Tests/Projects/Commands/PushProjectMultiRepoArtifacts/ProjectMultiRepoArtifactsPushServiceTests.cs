using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Commands.PushProjectMultiRepoArtifacts;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Microsoft.Extensions.Logging;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.PushProjectMultiRepoArtifacts;

public sealed class ProjectMultiRepoArtifactsPushServiceTests
{
    private const string PersonalAccessToken = "config-pat";
    private const string BranchName = "feature/generated-artifacts";
    private const string CommitMessage = "Push generated artifacts";

    private readonly IInfrastructureConfigRepository _configRepository;
    private readonly IGeneratedArtifactService _artifactService;
    private readonly IKeyVaultSecretClient _keyVaultSecretClient;
    private readonly IMultiScopeGitPushExecutor _pushExecutor;
    private readonly IRepositoryTargetResolver _targetResolver;
    private readonly ILogger<ProjectMultiRepoArtifactsPushService> _logger;
    private readonly Project _project;
    private readonly DomainInfrastructureConfig _config;
    private readonly InfraConfigRepositoryId _repositoryId;
    private readonly ResolvedRepositoryTarget _target;
    private readonly PushProjectMultiRepoArtifactsCommand _command;
    private readonly ProjectMultiRepoArtifactsPushService _sut;

    public ProjectMultiRepoArtifactsPushServiceTests()
    {
        _configRepository = Substitute.For<IInfrastructureConfigRepository>();
        _artifactService = Substitute.For<IGeneratedArtifactService>();
        _keyVaultSecretClient = Substitute.For<IKeyVaultSecretClient>();
        _pushExecutor = Substitute.For<IMultiScopeGitPushExecutor>();
        _targetResolver = Substitute.For<IRepositoryTargetResolver>();
        _logger = Substitute.For<ILogger<ProjectMultiRepoArtifactsPushService>>();

        _project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        _project.SetLayoutPreset(new LayoutPreset(LayoutPresetEnum.MultiRepo));
        _config = DomainInfrastructureConfig.Create(new Name("primary"), _project.Id);
        _config.SetLayoutMode(new ConfigLayoutMode(ConfigLayoutModeEnum.AllInOne));

        var contentKinds = RepositoryContentKinds.Create(
            RepositoryContentKindsEnum.Infrastructure | RepositoryContentKindsEnum.ApplicationCode);
        if (contentKinds.IsError)
            throw new InvalidOperationException(contentKinds.FirstError.Description);

        var repositoryResult = _config.AddRepository(
            new GitProviderType(GitProviderTypeEnum.GitHub),
            "https://github.com/octo-org/retail-platform",
            "main",
            contentKinds.Value);
        if (repositoryResult.IsError)
            throw new InvalidOperationException(repositoryResult.FirstError.Description);

        _repositoryId = repositoryResult.Value.Id;
        _target = new ResolvedRepositoryTarget(
            RepositoryId: _repositoryId.Value.ToString(),
            ProviderType: new GitProviderType(GitProviderTypeEnum.GitHub),
            RepositoryUrl: "https://github.com/octo-org/retail-platform",
            Owner: "octo-org",
            RepositoryName: "retail-platform",
            Branch: "main",
            BasePath: null,
            PipelineBasePath: null,
            PatSecretName: ProjectGitSecretNames.GetInfraConfigRepositoryPatSecretName(_repositoryId));
        _command = new PushProjectMultiRepoArtifactsCommand(
            _project.Id,
            [
                new InfrastructureConfigPushTarget(
                    _config.Id,
                    [new ConfigRepositoryPushTarget(_repositoryId, BranchName, CommitMessage)])
            ]);

        _sut = new ProjectMultiRepoArtifactsPushService(
            _configRepository,
            _artifactService,
            _keyVaultSecretClient,
            _pushExecutor,
            _targetResolver,
            _logger);
    }

    [Fact]
    public async Task Given_AllInOneConfigurationWithGeneratedArtifacts_When_PushAsync_Then_PushesAllArtifactsToItsConfigRepositoryAsync()
    {
        // Arrange
        ConfigureConfigRepository();
        ConfigureResolvedTarget();
        ConfigureGeneratedArtifacts();

        var pushResult = new PushBicepToGitResult(
            BranchName,
            "https://github.com/octo-org/retail-platform/tree/feature/generated-artifacts",
            "abc123",
            3);
        _pushExecutor.PushAsync(
                _target,
                Arg.Any<MultiScopeGitPushRequest>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(pushResult);

        // Act
        var result = await _sut.PushAsync(_command, _project, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Results.Should().ContainSingle().Which.Should().Match<ConfigRepositoryPushResult>(push =>
            push.InfrastructureConfigId == _config.Id
            && push.RepositoryId == _repositoryId
            && push.Success
            && push.BranchUrl == pushResult.BranchUrl
            && push.CommitSha == pushResult.CommitSha
            && push.FileCount == pushResult.FileCount);

        await _pushExecutor.Received(1).PushAsync(
            _target,
            Arg.Is<MultiScopeGitPushRequest>(request =>
                request.Token == PersonalAccessToken
                && request.Owner == _target.Owner
                && request.RepositoryName == _target.RepositoryName
                && request.BaseBranch == _target.Branch
                && request.TargetBranchName == BranchName
                && request.CommitMessage == CommitMessage
                && request.Scopes.Count == 1
                && request.Scopes[0].Files.ContainsKey("main.bicep")
                && request.Scopes[0].Files.ContainsKey("ci.pipeline.yml")
                && request.Scopes[0].Files.ContainsKey("bootstrap.pipeline.yml")),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_TwoAllInOneConfigurations_When_PushAsync_Then_PushesEachConfigurationToItsOwnRepositoryAsync()
    {
        // Arrange
        var secondConfig = CreateAllInOneConfiguration("secondary", "secondary-platform", _project);
        var secondRepository = secondConfig.Repositories.Single();
        var secondTarget = CreateTarget(secondRepository.Id, secondRepository.RepositoryName);
        var secondCommandTarget = new InfrastructureConfigPushTarget(
            secondConfig.Id,
            [new ConfigRepositoryPushTarget(secondRepository.Id, BranchName, CommitMessage)]);
        var command = new PushProjectMultiRepoArtifactsCommand(
            _project.Id,
            [_command.Configurations[0], secondCommandTarget]);

        _configRepository.GetByIdAsync(_config.Id, Arg.Any<CancellationToken>()).Returns(_config);
        _configRepository.GetByIdAsync(secondConfig.Id, Arg.Any<CancellationToken>()).Returns(secondConfig);
        _targetResolver.Resolve(_project, _config, ArtifactKind.Infrastructure).Returns(_target);
        _targetResolver.Resolve(_project, secondConfig, ArtifactKind.Infrastructure).Returns(secondTarget);
        _keyVaultSecretClient.GetSecretAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(PersonalAccessToken);
        ConfigureGeneratedArtifacts(_config.Id);
        ConfigureGeneratedArtifacts(secondConfig.Id);
        _pushExecutor.PushAsync(
                Arg.Any<ResolvedRepositoryTarget>(),
                Arg.Any<MultiScopeGitPushRequest>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(new PushBicepToGitResult(BranchName, "https://example/branch", "abc123", 3));

        // Act
        var result = await _sut.PushAsync(command, _project, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Results.Should().HaveCount(2);
        result.Value.Results.Select(push => push.InfrastructureConfigId)
            .Should().BeEquivalentTo([_config.Id, secondConfig.Id]);
        await _pushExecutor.Received(1).PushAsync(
            _target,
            Arg.Any<MultiScopeGitPushRequest>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
        await _pushExecutor.Received(1).PushAsync(
            secondTarget,
            Arg.Any<MultiScopeGitPushRequest>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_FirstRepositoryPushIsCanceled_When_PushAsync_Then_PropagatesCancellationAndDoesNotPushNextRepositoryAsync()
    {
        // Arrange
        var secondConfig = CreateAllInOneConfiguration("secondary", "secondary-platform", _project);
        var secondRepository = secondConfig.Repositories.Single();
        var secondTarget = CreateTarget(secondRepository.Id, secondRepository.RepositoryName);
        var command = new PushProjectMultiRepoArtifactsCommand(
            _project.Id,
            [
                _command.Configurations[0],
                new InfrastructureConfigPushTarget(
                    secondConfig.Id,
                    [new ConfigRepositoryPushTarget(secondRepository.Id, BranchName, CommitMessage)])
            ]);
        var cancellationToken = new CancellationToken(canceled: true);

        _configRepository.GetByIdAsync(_config.Id, Arg.Any<CancellationToken>()).Returns(_config);
        _configRepository.GetByIdAsync(secondConfig.Id, Arg.Any<CancellationToken>()).Returns(secondConfig);
        _targetResolver.Resolve(_project, _config, ArtifactKind.Infrastructure).Returns(_target);
        _targetResolver.Resolve(_project, secondConfig, ArtifactKind.Infrastructure).Returns(secondTarget);
        _keyVaultSecretClient.GetSecretAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(PersonalAccessToken);
        ConfigureGeneratedArtifacts(_config.Id);
        ConfigureGeneratedArtifacts(secondConfig.Id);
        _pushExecutor.PushAsync(
                Arg.Any<ResolvedRepositoryTarget>(),
                Arg.Any<MultiScopeGitPushRequest>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromCanceled<ErrorOr<PushBicepToGitResult>>(cancellationToken));
        var expectedFirstTarget = _config.Id.Value.CompareTo(secondConfig.Id.Value) < 0
            ? _target
            : secondTarget;
        var expectedSecondTarget = ReferenceEquals(expectedFirstTarget, _target)
            ? secondTarget
            : _target;

        // Act
        Func<Task> act = async () => await _sut.PushAsync(command, _project, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        await _pushExecutor.Received(1).PushAsync(
            expectedFirstTarget,
            Arg.Any<MultiScopeGitPushRequest>(),
            Arg.Any<string>(),
            cancellationToken);
        await _pushExecutor.DidNotReceive().PushAsync(
            expectedSecondTarget,
            Arg.Any<MultiScopeGitPushRequest>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_AConfigurationWithMissingBicepArtifacts_When_PushAsync_Then_DoesNotPushAnyConfigurationAsync()
    {
        // Arrange
        var secondConfig = CreateAllInOneConfiguration("secondary", "secondary-platform", _project);
        var secondRepository = secondConfig.Repositories.Single();
        var secondTarget = CreateTarget(secondRepository.Id, secondRepository.RepositoryName);
        var command = new PushProjectMultiRepoArtifactsCommand(
            _project.Id,
            [
                _command.Configurations[0],
                new InfrastructureConfigPushTarget(
                    secondConfig.Id,
                    [new ConfigRepositoryPushTarget(secondRepository.Id, BranchName, CommitMessage)])
            ]);

        _configRepository.GetByIdAsync(_config.Id, Arg.Any<CancellationToken>()).Returns(_config);
        _configRepository.GetByIdAsync(secondConfig.Id, Arg.Any<CancellationToken>()).Returns(secondConfig);
        _targetResolver.Resolve(_project, _config, ArtifactKind.Infrastructure).Returns(_target);
        _targetResolver.Resolve(_project, secondConfig, ArtifactKind.Infrastructure).Returns(secondTarget);
        ConfigureGeneratedArtifacts(_config.Id);
        _artifactService.GetLatestFilesAsync("bicep", secondConfig.Id.Value, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyDictionary<string, string>?)null);

        // Act
        var result = await _sut.PushAsync(command, _project, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(
            InfraFlowSculptor.Domain.Common.Errors.Errors.InfrastructureConfig
                .BicepFilesNotFoundError(secondConfig.Id.Value).Code);
        await _pushExecutor.DidNotReceive().PushAsync(
            Arg.Any<ResolvedRepositoryTarget>(),
            Arg.Any<MultiScopeGitPushRequest>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_RepositoryDoesNotBelongToConfiguration_When_PushAsync_Then_ReturnsErrorWithoutPushingAsync()
    {
        // Arrange
        var unknownRepositoryId = InfraConfigRepositoryId.CreateUnique();
        var command = new PushProjectMultiRepoArtifactsCommand(
            _project.Id,
            [
                new InfrastructureConfigPushTarget(
                    _config.Id,
                    [new ConfigRepositoryPushTarget(unknownRepositoryId, BranchName, CommitMessage)])
            ]);
        _configRepository.GetByIdAsync(_config.Id, Arg.Any<CancellationToken>()).Returns(_config);

        // Act
        var result = await _sut.PushAsync(command, _project, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(
            InfraFlowSculptor.Domain.Common.Errors.Errors.InfraConfigRepository
                .NotFound(unknownRepositoryId).Code);
        await _pushExecutor.DidNotReceive().PushAsync(
            Arg.Any<ResolvedRepositoryTarget>(),
            Arg.Any<MultiScopeGitPushRequest>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_SplitInfraCodeConfiguration_When_PushAsync_Then_SendsEachArtifactSetToItsRepositoryAsync()
    {
        // Arrange
        var splitConfig = CreateSplitConfiguration("split", "split-infra", "split-app", _project);
        var infraRepository = splitConfig.Repositories.Single(repository =>
            repository.ContentKinds.Has(RepositoryContentKindsEnum.Infrastructure));
        var appRepository = splitConfig.Repositories.Single(repository =>
            repository.ContentKinds.Has(RepositoryContentKindsEnum.ApplicationCode));
        var infraTarget = CreateTarget(infraRepository.Id, infraRepository.RepositoryName);
        var appTarget = CreateTarget(appRepository.Id, appRepository.RepositoryName);
        var command = new PushProjectMultiRepoArtifactsCommand(
            _project.Id,
            [
                new InfrastructureConfigPushTarget(
                    splitConfig.Id,
                    [
                        new ConfigRepositoryPushTarget(infraRepository.Id, BranchName, "Push infra artifacts"),
                        new ConfigRepositoryPushTarget(appRepository.Id, BranchName, "Push app artifacts")
                    ])
            ]);

        _configRepository.GetByIdAsync(splitConfig.Id, Arg.Any<CancellationToken>()).Returns(splitConfig);
        _targetResolver.Resolve(_project, splitConfig, ArtifactKind.Infrastructure).Returns(infraTarget);
        _targetResolver.Resolve(_project, splitConfig, ArtifactKind.ApplicationPipeline).Returns(appTarget);
        _keyVaultSecretClient.GetSecretAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(PersonalAccessToken);
        _artifactService.GetLatestFilesAsync("bicep", splitConfig.Id.Value, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string> { ["main.bicep"] = "resource" });
        _artifactService.GetLatestFilesAsync("pipeline", splitConfig.Id.Value, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string>
            {
                ["ci.pipeline.yml"] = "infra-pipeline",
                ["apps/api/ci.pipeline.yml"] = "app-pipeline"
            });
        _artifactService.GetLatestFilesAsync("bootstrap", splitConfig.Id.Value, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string>
            {
                ["infra/bootstrap.pipeline.yml"] = "infra-bootstrap",
                ["app/bootstrap.pipeline.yml"] = "app-bootstrap"
            });
        _pushExecutor.PushAsync(
                Arg.Any<ResolvedRepositoryTarget>(),
                Arg.Any<MultiScopeGitPushRequest>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(new PushBicepToGitResult(BranchName, "https://example/branch", "abc123", 2));

        // Act
        var result = await _sut.PushAsync(command, _project, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Results.Should().HaveCount(2);
        result.Value.Results.Select(push => push.RepositoryId)
            .Should().BeEquivalentTo([infraRepository.Id, appRepository.Id]);

        await _pushExecutor.Received(1).PushAsync(
            infraTarget,
            Arg.Is<MultiScopeGitPushRequest>(request =>
                request.Scopes.Count == 1
                && request.Scopes[0].Files.ContainsKey("main.bicep")
                && request.Scopes[0].Files.ContainsKey("ci.pipeline.yml")
                && request.Scopes[0].Files.ContainsKey("bootstrap.pipeline.yml")
                && request.Scopes[0].Files["bootstrap.pipeline.yml"] == "infra-bootstrap"),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
        await _pushExecutor.Received(1).PushAsync(
            appTarget,
            Arg.Is<MultiScopeGitPushRequest>(request =>
                request.Scopes.Count == 2
                && request.Scopes.SelectMany(scope => scope.Files)
                    .Any(file => file.Key == "api/ci.pipeline.yml" && file.Value == "app-pipeline")
                && request.Scopes.SelectMany(scope => scope.Files)
                    .Any(file => file.Key == "bootstrap.pipeline.yml" && file.Value == "app-bootstrap")
                && !request.Scopes.SelectMany(scope => scope.Files)
                    .Any(file => file.Key == "main.bicep")),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_SplitInfraCodeBootstrapIsMissingApplicationBucket_When_PushAsync_Then_ReturnsPrevalidationErrorWithoutPushingAsync()
    {
        // Arrange
        var splitConfig = CreateSplitConfiguration("split", "split-infra", "split-app", _project);
        var infraRepository = splitConfig.Repositories.Single(repository =>
            repository.ContentKinds.Has(RepositoryContentKindsEnum.Infrastructure));
        var appRepository = splitConfig.Repositories.Single(repository =>
            repository.ContentKinds.Has(RepositoryContentKindsEnum.ApplicationCode));
        var infraTarget = CreateTarget(infraRepository.Id, infraRepository.RepositoryName);
        var appTarget = CreateTarget(appRepository.Id, appRepository.RepositoryName);
        var command = CreateSplitCommand(_project.Id, splitConfig.Id, infraRepository.Id, appRepository.Id);

        ConfigureSplitRepositories(splitConfig, infraTarget, appTarget);
        _artifactService.GetLatestFilesAsync("bicep", splitConfig.Id.Value, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string> { ["main.bicep"] = "resource" });
        _artifactService.GetLatestFilesAsync("pipeline", splitConfig.Id.Value, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string>
            {
                ["ci.pipeline.yml"] = "infra-pipeline",
                ["apps/api/ci.pipeline.yml"] = "app-pipeline"
            });
        _artifactService.GetLatestFilesAsync("bootstrap", splitConfig.Id.Value, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string>
            {
                ["infra/bootstrap.pipeline.yml"] = "infra-bootstrap"
            });

        // Act
        var result = await _sut.PushAsync(command, _project, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(
            InfraFlowSculptor.Domain.Common.Errors.Errors.InfrastructureConfig
                .BootstrapFilesNotFoundError(splitConfig.Id.Value).Code);
        await _pushExecutor.DidNotReceive().PushAsync(
            Arg.Any<ResolvedRepositoryTarget>(),
            Arg.Any<MultiScopeGitPushRequest>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_SplitInfraCodeBootstrapContainsUnscopedFile_When_PushAsync_Then_ReturnsPrevalidationErrorWithoutPushingAsync()
    {
        // Arrange
        var splitConfig = CreateSplitConfiguration("split", "split-infra", "split-app", _project);
        var infraRepository = splitConfig.Repositories.Single(repository =>
            repository.ContentKinds.Has(RepositoryContentKindsEnum.Infrastructure));
        var appRepository = splitConfig.Repositories.Single(repository =>
            repository.ContentKinds.Has(RepositoryContentKindsEnum.ApplicationCode));
        var infraTarget = CreateTarget(infraRepository.Id, infraRepository.RepositoryName);
        var appTarget = CreateTarget(appRepository.Id, appRepository.RepositoryName);
        var command = CreateSplitCommand(_project.Id, splitConfig.Id, infraRepository.Id, appRepository.Id);

        ConfigureSplitRepositories(splitConfig, infraTarget, appTarget);
        _artifactService.GetLatestFilesAsync("bicep", splitConfig.Id.Value, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string> { ["main.bicep"] = "resource" });
        _artifactService.GetLatestFilesAsync("pipeline", splitConfig.Id.Value, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string>
            {
                ["ci.pipeline.yml"] = "infra-pipeline",
                ["apps/api/ci.pipeline.yml"] = "app-pipeline"
            });
        _artifactService.GetLatestFilesAsync("bootstrap", splitConfig.Id.Value, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string>
            {
                ["bootstrap.pipeline.yml"] = "ambiguous-bootstrap",
                ["app/bootstrap.pipeline.yml"] = "app-bootstrap"
            });

        // Act
        var result = await _sut.PushAsync(command, _project, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(
            InfraFlowSculptor.Domain.Common.Errors.Errors.InfrastructureConfig
                .BootstrapFilesNotFoundError(splitConfig.Id.Value).Code);
        await _pushExecutor.DidNotReceive().PushAsync(
            Arg.Any<ResolvedRepositoryTarget>(),
            Arg.Any<MultiScopeGitPushRequest>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_FirstRepositoryPushFails_When_PushAsync_Then_ReturnsFailureAndContinuesWithNextRepositoryAsync()
    {
        // Arrange
        var secondConfig = CreateAllInOneConfiguration("secondary", "secondary-platform", _project);
        var secondRepository = secondConfig.Repositories.Single();
        var secondTarget = CreateTarget(secondRepository.Id, secondRepository.RepositoryName);
        var command = new PushProjectMultiRepoArtifactsCommand(
            _project.Id,
            [
                _command.Configurations[0],
                new InfrastructureConfigPushTarget(
                    secondConfig.Id,
                    [new ConfigRepositoryPushTarget(secondRepository.Id, BranchName, CommitMessage)])
            ]);
        var pushError = InfraFlowSculptor.Domain.Common.Errors.Errors.GitRepository.PushFailed(
            "The first repository rejected the push.");

        _configRepository.GetByIdAsync(_config.Id, Arg.Any<CancellationToken>()).Returns(_config);
        _configRepository.GetByIdAsync(secondConfig.Id, Arg.Any<CancellationToken>()).Returns(secondConfig);
        _targetResolver.Resolve(_project, _config, ArtifactKind.Infrastructure).Returns(_target);
        _targetResolver.Resolve(_project, secondConfig, ArtifactKind.Infrastructure).Returns(secondTarget);
        _keyVaultSecretClient.GetSecretAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(PersonalAccessToken);
        ConfigureGeneratedArtifacts(_config.Id);
        ConfigureGeneratedArtifacts(secondConfig.Id);
        _pushExecutor.PushAsync(
                Arg.Any<ResolvedRepositoryTarget>(),
                Arg.Any<MultiScopeGitPushRequest>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<ResolvedRepositoryTarget>().RepositoryId == _target.RepositoryId
                ? pushError
                : new PushBicepToGitResult(BranchName, "https://example/second", "second-sha", 3));

        // Act
        var result = await _sut.PushAsync(command, _project, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Results.Should().HaveCount(2);
        result.Value.Results.Single(push => push.RepositoryId == _repositoryId).Success.Should().BeFalse();
        result.Value.Results.Single(push => push.RepositoryId == secondRepository.Id).Success.Should().BeTrue();
        await _pushExecutor.Received(1).PushAsync(
            _target,
            Arg.Any<MultiScopeGitPushRequest>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
        await _pushExecutor.Received(1).PushAsync(
            secondTarget,
            Arg.Any<MultiScopeGitPushRequest>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ConfigRepositoryHasNoPatSecret_When_PushAsync_Then_ReturnsFailureWithoutUsingProjectSecretFallbackAsync()
    {
        // Arrange
        var targetWithoutSecret = _target with { PatSecretName = null };
        ConfigureConfigRepository();
        _targetResolver.Resolve(_project, _config, ArtifactKind.Infrastructure).Returns(targetWithoutSecret);
        ConfigureGeneratedArtifacts();

        // Act
        var result = await _sut.PushAsync(_command, _project, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Results.Should().ContainSingle().Which.Should().Match<ConfigRepositoryPushResult>(push =>
            !push.Success
            && push.ErrorCode == InfraFlowSculptor.Domain.Common.Errors.Errors.GitRepository
                .SecretRetrievalFailed().Code);
        await _keyVaultSecretClient.DidNotReceive()
            .GetSecretAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _pushExecutor.DidNotReceive().PushAsync(
            Arg.Any<ResolvedRepositoryTarget>(),
            Arg.Any<MultiScopeGitPushRequest>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_AllInOneArtifactsContainConflictingPaths_When_PushAsync_Then_ReturnsCollisionWithoutPushingAsync()
    {
        // Arrange
        ConfigureConfigRepository();
        ConfigureResolvedTarget();
        _artifactService.GetLatestFilesAsync("bicep", _config.Id.Value, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string> { ["main.bicep"] = "resource" });
        _artifactService.GetLatestFilesAsync("pipeline", _config.Id.Value, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string> { ["main.bicep"] = "pipeline" });
        _artifactService.GetLatestFilesAsync("bootstrap", _config.Id.Value, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string> { ["bootstrap.pipeline.yml"] = "bootstrap" });

        // Act
        var result = await _sut.PushAsync(_command, _project, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(
            InfraFlowSculptor.Domain.Common.Errors.Errors.GitRepository
                .PushFailed("Generated file collision detected for path 'main.bicep'.").Code);
        await _pushExecutor.DidNotReceive().PushAsync(
            Arg.Any<ResolvedRepositoryTarget>(),
            Arg.Any<MultiScopeGitPushRequest>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_SecondConfigurationContainsConflictingPaths_When_PushAsync_Then_DoesNotPushFirstConfigurationAsync()
    {
        // Arrange
        var secondConfig = CreateAllInOneConfiguration("secondary", "secondary-platform", _project);
        var secondRepository = secondConfig.Repositories.Single();
        var secondTarget = CreateTarget(secondRepository.Id, secondRepository.RepositoryName);
        var command = new PushProjectMultiRepoArtifactsCommand(
            _project.Id,
            [
                _command.Configurations[0],
                new InfrastructureConfigPushTarget(
                    secondConfig.Id,
                    [new ConfigRepositoryPushTarget(secondRepository.Id, BranchName, CommitMessage)])
            ]);

        _configRepository.GetByIdAsync(_config.Id, Arg.Any<CancellationToken>()).Returns(_config);
        _configRepository.GetByIdAsync(secondConfig.Id, Arg.Any<CancellationToken>()).Returns(secondConfig);
        _targetResolver.Resolve(_project, _config, ArtifactKind.Infrastructure).Returns(_target);
        _targetResolver.Resolve(_project, secondConfig, ArtifactKind.Infrastructure).Returns(secondTarget);
        _keyVaultSecretClient.GetSecretAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(PersonalAccessToken);
        ConfigureGeneratedArtifacts(_config.Id);
        _artifactService.GetLatestFilesAsync("bicep", secondConfig.Id.Value, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string> { ["main.bicep"] = "second-resource" });
        _artifactService.GetLatestFilesAsync("pipeline", secondConfig.Id.Value, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string> { ["main.bicep"] = "second-pipeline" });
        _artifactService.GetLatestFilesAsync("bootstrap", secondConfig.Id.Value, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string> { ["bootstrap.pipeline.yml"] = "second-bootstrap" });

        // Act
        var result = await _sut.PushAsync(command, _project, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(
            InfraFlowSculptor.Domain.Common.Errors.Errors.GitRepository
                .PushFailed("Generated file collision detected for path 'main.bicep'.").Code);
        await _pushExecutor.DidNotReceive().PushAsync(
            Arg.Any<ResolvedRepositoryTarget>(),
            Arg.Any<MultiScopeGitPushRequest>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_GitProviderThrowsUnexpectedException_When_PushAsync_Then_HidesExceptionDetailsFromResultAndLogsAsync()
    {
        // Arrange
        ConfigureConfigRepository();
        ConfigureResolvedTarget();
        ConfigureGeneratedArtifacts();
        const string sensitiveProviderMessage = "internal transport details https://provider.example/secret";
        _pushExecutor.PushAsync(
                Arg.Any<ResolvedRepositoryTarget>(),
                Arg.Any<MultiScopeGitPushRequest>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ErrorOr<PushBicepToGitResult>>(
                new InvalidOperationException(sensitiveProviderMessage)));

        // Act
        var result = await _sut.PushAsync(_command, _project, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Results.Should().ContainSingle().Which.Should().Match<ConfigRepositoryPushResult>(push =>
            !push.Success
            && push.ErrorCode == "GitProvider.UnexpectedError"
            && push.ErrorDescription == "The repository push could not be completed."
            && !push.ErrorDescription.Contains(sensitiveProviderMessage, StringComparison.Ordinal));
        _logger.ReceivedWithAnyArgs().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    private void ConfigureConfigRepository()
    {
        _configRepository.GetByIdAsync(_config.Id, Arg.Any<CancellationToken>()).Returns(_config);
    }

    private void ConfigureResolvedTarget()
    {
        _targetResolver.Resolve(_project, _config, ArtifactKind.Infrastructure).Returns(_target);
        _keyVaultSecretClient.GetSecretAsync(_target.PatSecretName!, Arg.Any<CancellationToken>())
            .Returns(PersonalAccessToken);
    }

    private void ConfigureGeneratedArtifacts()
    {
        ConfigureGeneratedArtifacts(_config.Id);
    }

    private void ConfigureGeneratedArtifacts(InfrastructureConfigId configId)
    {
        _artifactService.GetLatestFilesAsync("bicep", configId.Value, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string> { ["main.bicep"] = "resource" });
        _artifactService.GetLatestFilesAsync("pipeline", configId.Value, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string> { ["ci.pipeline.yml"] = "pipeline" });
        _artifactService.GetLatestFilesAsync("bootstrap", configId.Value, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string> { ["bootstrap.pipeline.yml"] = "bootstrap" });
    }

    private static DomainInfrastructureConfig CreateAllInOneConfiguration(
        string configName,
        string repositoryName,
        Project? project = null)
    {
        var parentProject = project ?? Project.Create(
            new Name("Retail Platform"),
            "Provision retail assets.",
            UserId.CreateUnique());
        var config = DomainInfrastructureConfig.Create(new Name(configName), parentProject.Id);
        config.SetLayoutMode(new ConfigLayoutMode(ConfigLayoutModeEnum.AllInOne));

        var contentKinds = RepositoryContentKinds.Create(
            RepositoryContentKindsEnum.Infrastructure | RepositoryContentKindsEnum.ApplicationCode);
        if (contentKinds.IsError)
            throw new InvalidOperationException(contentKinds.FirstError.Description);

        var repositoryResult = config.AddRepository(
            new GitProviderType(GitProviderTypeEnum.GitHub),
            $"https://github.com/octo-org/{repositoryName}",
            "main",
            contentKinds.Value);
        if (repositoryResult.IsError)
            throw new InvalidOperationException(repositoryResult.FirstError.Description);

        return config;
    }

    private static DomainInfrastructureConfig CreateSplitConfiguration(
        string configName,
        string infraRepositoryName,
        string appRepositoryName,
        Project project)
    {
        var config = DomainInfrastructureConfig.Create(new Name(configName), project.Id);
        config.SetLayoutMode(new ConfigLayoutMode(ConfigLayoutModeEnum.SplitInfraCode));

        var infraKinds = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure);
        var infraRepositoryResult = config.AddRepository(
            new GitProviderType(GitProviderTypeEnum.GitHub),
            $"https://github.com/octo-org/{infraRepositoryName}",
            "main",
            infraKinds.Value);
        if (infraRepositoryResult.IsError)
            throw new InvalidOperationException(infraRepositoryResult.FirstError.Description);

        var appKinds = RepositoryContentKinds.Create(RepositoryContentKindsEnum.ApplicationCode);
        var appRepositoryResult = config.AddRepository(
            new GitProviderType(GitProviderTypeEnum.GitHub),
            $"https://github.com/octo-org/{appRepositoryName}",
            "main",
            appKinds.Value);
        if (appRepositoryResult.IsError)
            throw new InvalidOperationException(appRepositoryResult.FirstError.Description);

        return config;
    }

    private void ConfigureSplitRepositories(
        DomainInfrastructureConfig config,
        ResolvedRepositoryTarget infraTarget,
        ResolvedRepositoryTarget appTarget)
    {
        _configRepository.GetByIdAsync(config.Id, Arg.Any<CancellationToken>()).Returns(config);
        _targetResolver.Resolve(_project, config, ArtifactKind.Infrastructure).Returns(infraTarget);
        _targetResolver.Resolve(_project, config, ArtifactKind.ApplicationPipeline).Returns(appTarget);
        _keyVaultSecretClient.GetSecretAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(PersonalAccessToken);
    }

    private static PushProjectMultiRepoArtifactsCommand CreateSplitCommand(
        ProjectId projectId,
        InfrastructureConfigId configId,
        InfraConfigRepositoryId infraRepositoryId,
        InfraConfigRepositoryId appRepositoryId) =>
        new(
            ProjectId: projectId,
            Configurations:
            [
                new InfrastructureConfigPushTarget(
                    configId,
                    [
                        new ConfigRepositoryPushTarget(infraRepositoryId, BranchName, "Push infra artifacts"),
                        new ConfigRepositoryPushTarget(appRepositoryId, BranchName, "Push app artifacts")
                    ])
            ]);

    private static ResolvedRepositoryTarget CreateTarget(
        InfraConfigRepositoryId repositoryId,
        string repositoryName) =>
        new(
            RepositoryId: repositoryId.Value.ToString(),
            ProviderType: new GitProviderType(GitProviderTypeEnum.GitHub),
            RepositoryUrl: $"https://github.com/octo-org/{repositoryName}",
            Owner: "octo-org",
            RepositoryName: repositoryName,
            Branch: "main",
            BasePath: null,
            PipelineBasePath: null,
            PatSecretName: ProjectGitSecretNames.GetInfraConfigRepositoryPatSecretName(repositoryId));
}