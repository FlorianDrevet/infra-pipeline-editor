using FluentAssertions;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Application.Projects.Queries.ListCodeRepoBranches;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Queries.ListCodeRepoBranches;

public sealed class ListCodeRepoBranchesQueryHandlerTests
{
    private readonly IInfrastructureConfigRepository _infraConfigRepository;
    private readonly IProjectAccessService _accessService;
    private readonly IProjectRepository _projectRepository;
    private readonly Project _project;
    private readonly DomainInfrastructureConfig _config;
    private readonly ListCodeRepoBranchesQuery _query;
    private readonly ListCodeRepoBranchesQueryHandler _sut;

    public ListCodeRepoBranchesQueryHandlerTests()
    {
        _projectRepository = Substitute.For<IProjectRepository>();
        _infraConfigRepository = Substitute.For<IInfrastructureConfigRepository>();
        _accessService = Substitute.For<IProjectAccessService>();
        var keyVaultSecretClient = Substitute.For<IKeyVaultSecretClient>();
        var gitProviderFactory = Substitute.For<IGitProviderFactory>();
        var targetResolver = Substitute.For<IRepositoryTargetResolver>();

        _project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        _config = DomainInfrastructureConfig.Create(new Name("primary"), _project.Id);
        _query = new ListCodeRepoBranchesQuery(_project.Id, _config.Id);

        var helper = new GitRepoQueryHelper(
            _projectRepository,
            _infraConfigRepository,
            _accessService,
            keyVaultSecretClient,
            gitProviderFactory,
            targetResolver);
        _sut = new ListCodeRepoBranchesQueryHandler(helper);
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
