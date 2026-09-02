using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddInfraConfigRepository;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.AddInfraConfigRepository;

public sealed class AddInfraConfigRepositoryCommandHandlerTests
{
    private readonly IInfrastructureConfigRepository _infrastructureConfigRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectAccessService _accessService;
    private readonly IKeyVaultSecretClient _keyVaultSecretClient;
    private readonly IGitProviderFactory _gitProviderFactory;
    private readonly IGitProviderService _gitProviderService;
    private readonly Project _project;
    private readonly DomainInfrastructureConfig _config;
    private readonly AddInfraConfigRepositoryCommandHandler _sut;

    public AddInfraConfigRepositoryCommandHandlerTests()
    {
        _infrastructureConfigRepository = Substitute.For<IInfrastructureConfigRepository>();
        _projectRepository = Substitute.For<IProjectRepository>();
        _accessService = Substitute.For<IProjectAccessService>();
        _keyVaultSecretClient = Substitute.For<IKeyVaultSecretClient>();
        _gitProviderFactory = Substitute.For<IGitProviderFactory>();
        _gitProviderService = Substitute.For<IGitProviderService>();

        _project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        _config = DomainInfrastructureConfig.Create(new Name("primary"), _project.Id);
        _config.SetLayoutMode(new ConfigLayoutMode(ConfigLayoutModeEnum.SplitInfraCode));

        _accessService.VerifyOwnerAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdAsync(_project.Id)
            .Returns(_project);
        _infrastructureConfigRepository.GetByIdAsync(_config.Id)
            .Returns(_config);
        _gitProviderFactory.Create(Arg.Any<GitProviderType>())
            .Returns(_gitProviderService);
        _gitProviderService.ListBranchesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Branches(new GitBranchResult("main", false)));
        _keyVaultSecretClient.SetSecretAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        _sut = new AddInfraConfigRepositoryCommandHandler(
            _infrastructureConfigRepository,
            _projectRepository,
            _accessService,
            _keyVaultSecretClient,
            _gitProviderFactory);
    }

    [Fact]
    public async Task Given_InvalidProviderType_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        var command = new AddInfraConfigRepositoryCommand(
            _project.Id,
            _config.Id,
            ProviderType: "Unsupported",
            RepositoryUrl: "https://github.com/floriandrevet/infra",
            DefaultBranch: "main",
            PersonalAccessToken: "token",
            ContentKinds: [nameof(RepositoryContentKindsEnum.Infrastructure)]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.GitRepository.InvalidProviderType("Unsupported").Code);
        _infrastructureConfigRepository.DidNotReceive().Update(Arg.Any<DomainInfrastructureConfig>());
    }

    [Fact]
    public async Task Given_InvalidContentKinds_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        var command = new AddInfraConfigRepositoryCommand(
            _project.Id,
            _config.Id,
            ProviderType: nameof(GitProviderTypeEnum.GitHub),
            RepositoryUrl: "https://github.com/floriandrevet/infra",
            DefaultBranch: "main",
            PersonalAccessToken: "token",
            ContentKinds: ["Unsupported"]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.ProjectRepository.NoContentKind().Code);
        _infrastructureConfigRepository.DidNotReceive().Update(Arg.Any<DomainInfrastructureConfig>());
    }

    [Fact]
    public async Task Given_ConfiguredRepositoryWithoutPersonalAccessToken_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        var command = new AddInfraConfigRepositoryCommand(
            _project.Id,
            _config.Id,
            ProviderType: nameof(GitProviderTypeEnum.GitHub),
            RepositoryUrl: "https://github.com/floriandrevet/infra",
            DefaultBranch: "main",
            PersonalAccessToken: null,
            ContentKinds: [nameof(RepositoryContentKindsEnum.Infrastructure)]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.ProjectRepository.PersonalAccessTokenRequired().Code);
        await _gitProviderService.DidNotReceive().ListBranchesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        _infrastructureConfigRepository.DidNotReceive().Update(Arg.Any<DomainInfrastructureConfig>());
    }

    [Fact]
    public async Task Given_DefaultBranchMissingFromRemote_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        _gitProviderService.ListBranchesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Branches(new GitBranchResult("develop", false)));
        var command = new AddInfraConfigRepositoryCommand(
            _project.Id,
            _config.Id,
            ProviderType: nameof(GitProviderTypeEnum.GitHub),
            RepositoryUrl: "https://github.com/floriandrevet/infra",
            DefaultBranch: "main",
            PersonalAccessToken: "token",
            ContentKinds: [nameof(RepositoryContentKindsEnum.Infrastructure)]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.ProjectRepository.DefaultBranchNotFound("main").Code);
        _infrastructureConfigRepository.DidNotReceive().Update(Arg.Any<DomainInfrastructureConfig>());
    }

    [Fact]
    public async Task Given_IncompleteConnectionDetails_When_Handle_Then_DoesNotRequirePersonalAccessTokenAsync()
    {
        // Arrange: RepositoryUrl left empty means connection details are incomplete.
        // InfrastructureConfig.AddRepository (unlike Project.AddRepository) does not support
        // a partial/slot-style declaration, so the domain call still fails downstream — but the
        // PAT requirement and remote branch verification must never be triggered for it.
        var command = new AddInfraConfigRepositoryCommand(
            _project.Id,
            _config.Id,
            ProviderType: nameof(GitProviderTypeEnum.GitHub),
            RepositoryUrl: string.Empty,
            DefaultBranch: "main",
            PersonalAccessToken: null,
            ContentKinds: [nameof(RepositoryContentKindsEnum.Infrastructure)]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().NotBe(Errors.ProjectRepository.PersonalAccessTokenRequired().Code);
        await _gitProviderService.DidNotReceive().ListBranchesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _keyVaultSecretClient.DidNotReceive().SetSecretAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        _infrastructureConfigRepository.DidNotReceive().Update(Arg.Any<DomainInfrastructureConfig>());
    }

    [Fact]
    public async Task Given_ConfiguredRepositoryWithValidBranch_When_Handle_Then_StoresSecretAndAddsRepositoryAsync()
    {
        // Arrange
        const string personalAccessToken = "token";
        var command = new AddInfraConfigRepositoryCommand(
            _project.Id,
            _config.Id,
            ProviderType: nameof(GitProviderTypeEnum.GitHub),
            RepositoryUrl: "https://github.com/floriandrevet/infra",
            DefaultBranch: "main",
            PersonalAccessToken: personalAccessToken,
            ContentKinds: [nameof(RepositoryContentKindsEnum.Infrastructure)]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Value.Should().NotBeEmpty();
        await _gitProviderService.Received(1).ListBranchesAsync(personalAccessToken, "floriandrevet", "infra", Arg.Any<CancellationToken>());
        await _keyVaultSecretClient.Received(1).SetSecretAsync(
            Arg.Is<string>(secretName => secretName.Contains(result.Value.Value.ToString(), StringComparison.Ordinal)),
            personalAccessToken,
            Arg.Any<CancellationToken>());
        _infrastructureConfigRepository.Received(1).Update(_config);
    }

    private static Task<ErrorOr<IReadOnlyList<GitBranchResult>>> Branches(params GitBranchResult[] branches)
    {
        ErrorOr<IReadOnlyList<GitBranchResult>> result = branches;
        return Task.FromResult(result);
    }
}
