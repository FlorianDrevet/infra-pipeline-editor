using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.UpdateInfraConfigRepository;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.UpdateInfraConfigRepository;

public sealed class UpdateInfraConfigRepositoryCommandHandlerTests
{
    private readonly IInfrastructureConfigRepository _infrastructureConfigRepository;
    private readonly IProjectAccessService _accessService;
    private readonly IKeyVaultSecretClient _keyVaultSecretClient;
    private readonly IGitProviderFactory _gitProviderFactory;
    private readonly IGitProviderService _gitProviderService;
    private readonly Project _project;
    private readonly DomainInfrastructureConfig _config;
    private readonly InfraConfigRepository _repository;
    private readonly UpdateInfraConfigRepositoryCommandHandler _sut;

    public UpdateInfraConfigRepositoryCommandHandlerTests()
    {
        _infrastructureConfigRepository = Substitute.For<IInfrastructureConfigRepository>();
        _accessService = Substitute.For<IProjectAccessService>();
        _keyVaultSecretClient = Substitute.For<IKeyVaultSecretClient>();
        _gitProviderFactory = Substitute.For<IGitProviderFactory>();
        _gitProviderService = Substitute.For<IGitProviderService>();

        _project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        _config = DomainInfrastructureConfig.Create(new Name("primary"), _project.Id);
        _config.SetLayoutMode(new ConfigLayoutMode(ConfigLayoutModeEnum.SplitInfraCode));
        _repository = AddRepository(_config);

        _accessService.VerifyOwnerAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _infrastructureConfigRepository.GetByIdAsync(_config.Id)
            .Returns(_config);
        _keyVaultSecretClient.GetSecretAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("existing-token");
        _keyVaultSecretClient.SetSecretAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success);
        _gitProviderFactory.Create(Arg.Any<GitProviderType>())
            .Returns(_gitProviderService);
        _gitProviderService.ListBranchesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Branches(new GitBranchResult("main", false)));

        _sut = new UpdateInfraConfigRepositoryCommandHandler(
            _infrastructureConfigRepository,
            _accessService,
            _keyVaultSecretClient,
            _gitProviderFactory);
    }

    [Fact]
    public async Task Given_InvalidProviderType_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        var command = new UpdateInfraConfigRepositoryCommand(
            _project.Id,
            _config.Id,
            _repository.Id,
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
        var command = new UpdateInfraConfigRepositoryCommand(
            _project.Id,
            _config.Id,
            _repository.Id,
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
    public async Task Given_ConfiguredRepositoryWithoutNewToken_When_Handle_Then_UsesStoredTokenForBranchValidationAsync()
    {
        // Arrange
        var command = new UpdateInfraConfigRepositoryCommand(
            _project.Id,
            _config.Id,
            _repository.Id,
            ProviderType: nameof(GitProviderTypeEnum.GitHub),
            RepositoryUrl: "https://github.com/floriandrevet/infra",
            DefaultBranch: "main",
            PersonalAccessToken: null,
            ContentKinds: [nameof(RepositoryContentKindsEnum.Infrastructure)]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        await _gitProviderService.Received(1).ListBranchesAsync("existing-token", "floriandrevet", "infra", Arg.Any<CancellationToken>());
        await _keyVaultSecretClient.DidNotReceive().SetSecretAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        _infrastructureConfigRepository.Received(1).Update(_config);
    }

    [Fact]
    public async Task Given_ConfiguredRepositoryWithNewToken_When_Handle_Then_StoresNewTokenAfterBranchValidationAsync()
    {
        // Arrange
        const string personalAccessToken = "new-token";
        var command = new UpdateInfraConfigRepositoryCommand(
            _project.Id,
            _config.Id,
            _repository.Id,
            ProviderType: nameof(GitProviderTypeEnum.GitHub),
            RepositoryUrl: "https://github.com/floriandrevet/infra",
            DefaultBranch: "main",
            PersonalAccessToken: personalAccessToken,
            ContentKinds: [nameof(RepositoryContentKindsEnum.Infrastructure)]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        await _gitProviderService.Received(1).ListBranchesAsync(personalAccessToken, "floriandrevet", "infra", Arg.Any<CancellationToken>());
        await _keyVaultSecretClient.Received(1).SetSecretAsync(
            Arg.Is<string>(secretName => secretName.Contains(_repository.Id.Value.ToString(), StringComparison.Ordinal)),
            personalAccessToken,
            Arg.Any<CancellationToken>());
        _infrastructureConfigRepository.Received(1).Update(_config);
    }

    [Fact]
    public async Task Given_DefaultBranchMissingFromRemote_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        _gitProviderService.ListBranchesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Branches(new GitBranchResult("develop", false)));
        var command = new UpdateInfraConfigRepositoryCommand(
            _project.Id,
            _config.Id,
            _repository.Id,
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
    public async Task Given_BranchVerificationFails_When_Handle_Then_DoesNotCallUpdateAsync()
    {
        // Arrange
        _gitProviderService.ListBranchesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<IReadOnlyList<GitBranchResult>>>(
                Errors.ProjectRepository.DefaultBranchNotFound("main")));
        var command = new UpdateInfraConfigRepositoryCommand(
            _project.Id,
            _config.Id,
            _repository.Id,
            ProviderType: nameof(GitProviderTypeEnum.GitHub),
            RepositoryUrl: "https://github.com/floriandrevet/infra",
            DefaultBranch: "main",
            PersonalAccessToken: "token",
            ContentKinds: [nameof(RepositoryContentKindsEnum.Infrastructure)]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        _infrastructureConfigRepository.DidNotReceive().Update(Arg.Any<DomainInfrastructureConfig>());
    }

    private static InfraConfigRepository AddRepository(DomainInfrastructureConfig config)
    {
        var contentKinds = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;

        return config.AddRepository(
            new GitProviderType(GitProviderTypeEnum.GitHub),
            "https://github.com/floriandrevet/original",
            "main",
            contentKinds).Value;
    }

    private static Task<ErrorOr<IReadOnlyList<GitBranchResult>>> Branches(params GitBranchResult[] branches)
    {
        ErrorOr<IReadOnlyList<GitBranchResult>> result = branches;
        return Task.FromResult(result);
    }
}
