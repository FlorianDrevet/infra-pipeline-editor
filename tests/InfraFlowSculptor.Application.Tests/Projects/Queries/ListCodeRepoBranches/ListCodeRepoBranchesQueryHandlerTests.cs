using FluentAssertions;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Queries.ListCodeRepoBranches;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Queries.ListCodeRepoBranches;

public sealed class ListCodeRepoBranchesQueryHandlerTests
{
    private readonly IProjectRepository _projectRepository;
    private readonly IInfrastructureConfigRepository _infraConfigRepository;
    private readonly IProjectAccessService _accessService;
    private readonly IKeyVaultSecretClient _keyVaultSecretClient;
    private readonly IGitProviderFactory _gitProviderFactory;
    private readonly IRepositoryTargetResolver _targetResolver;
    private readonly Project _project;
    private readonly DomainInfrastructureConfig _config;
    private readonly ListCodeRepoBranchesQuery _query;
    private readonly ListCodeRepoBranchesQueryHandler _sut;

    public ListCodeRepoBranchesQueryHandlerTests()
    {
        _projectRepository = Substitute.For<IProjectRepository>();
        _infraConfigRepository = Substitute.For<IInfrastructureConfigRepository>();
        _accessService = Substitute.For<IProjectAccessService>();
        _keyVaultSecretClient = Substitute.For<IKeyVaultSecretClient>();
        _gitProviderFactory = Substitute.For<IGitProviderFactory>();
        _targetResolver = Substitute.For<IRepositoryTargetResolver>();

        _project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        _config = DomainInfrastructureConfig.Create(new Name("primary"), _project.Id);
        _query = new ListCodeRepoBranchesQuery(_project.Id, _config.Id);
        _sut = new ListCodeRepoBranchesQueryHandler(
            _projectRepository,
            _infraConfigRepository,
            _accessService,
            _keyVaultSecretClient,
            _gitProviderFactory,
            _targetResolver);
    }

    [Fact]
    public async Task Given_ConfigScopeLookup_When_Handle_Then_UsesDetachedConfigLookupAsync()
    {
        // Arrange
        _accessService.VerifyReadAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _infraConfigRepository.GetByIdReadOnlyAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns((DomainInfrastructureConfig?)null);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        await _infraConfigRepository.Received(1)
            .GetByIdReadOnlyAsync(_config.Id, Arg.Any<CancellationToken>());
        await _infraConfigRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<InfraFlowSculptor.Domain.Common.Models.ValueObject>(), Arg.Any<CancellationToken>());
    }
}
