using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListMyInfraConfigs;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Queries.ListMyInfraConfigs;

public sealed class ListMyInfrastructureConfigsQueryHandlerTests
{
    private readonly IProjectRepository _projectRepository;
    private readonly IInfrastructureConfigRepository _configRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;
    private readonly UserId _userId;
    private readonly ListMyInfrastructureConfigsQueryHandler _sut;

    public ListMyInfrastructureConfigsQueryHandlerTests()
    {
        _projectRepository = Substitute.For<IProjectRepository>();
        _configRepository = Substitute.For<IInfrastructureConfigRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _currentUser = Substitute.For<ICurrentUser>();
        _mapper = Substitute.For<IMapper>();
        _userId = UserId.CreateUnique();
        _currentUser.GetUserIdAsync(Arg.Any<CancellationToken>()).Returns(_userId);
        _sut = new ListMyInfrastructureConfigsQueryHandler(
            _projectRepository, _configRepository, _resourceGroupRepository, _currentUser, _mapper);
    }

    [Fact]
    public async Task Given_UserHasNoProjects_When_Handle_Then_ReturnsEmptyListAsync()
    {
        // Arrange
        _projectRepository.GetProjectIdsForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new List<ProjectId>());
        var query = new ListMyInfrastructureConfigsQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_UserHasProjectsWithConfigs_When_Handle_Then_ReturnsMappedConfigsAsync()
    {
        // Arrange
        var projectId = ProjectId.CreateUnique();
        var config = DomainInfrastructureConfig.Create(new Name("Config1"), projectId);
        var configResult = new GetInfrastructureConfigResult(
            config.Id, new Name("Config1"), projectId, null, true,
            [], [], []);

        _projectRepository.GetProjectIdsForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new List<ProjectId> { projectId });
        _configRepository.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<DomainInfrastructureConfig> { config });
        _mapper.Map<GetInfrastructureConfigResult>(config).Returns(configResult);
        _resourceGroupRepository.GetResourceCountsByInfraConfigIdsAsync(
                Arg.Any<List<InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.InfrastructureConfigId>>(),
                Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, (int ResourceGroupCount, int ResourceCount)>());
        var query = new ListMyInfrastructureConfigsQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
        result.Value[0].Name.Value.Should().Be("Config1");
    }

    [Fact]
    public async Task Given_ResourceCounts_When_Handle_Then_EnrichesResultsWithCountsAsync()
    {
        // Arrange
        var projectId = ProjectId.CreateUnique();
        var config = DomainInfrastructureConfig.Create(new Name("Config1"), projectId);
        var configResult = new GetInfrastructureConfigResult(
            config.Id, new Name("Config1"), projectId, null, true,
            [], [], []);

        _projectRepository.GetProjectIdsForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new List<ProjectId> { projectId });
        _configRepository.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<DomainInfrastructureConfig> { config });
        _mapper.Map<GetInfrastructureConfigResult>(config).Returns(configResult);

        var counts = new Dictionary<Guid, (int ResourceGroupCount, int ResourceCount)>
        {
            { config.Id.Value, (3, 10) }
        };
        _resourceGroupRepository.GetResourceCountsByInfraConfigIdsAsync(
                Arg.Any<List<InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.InfrastructureConfigId>>(),
                Arg.Any<CancellationToken>())
            .Returns(counts);

        var query = new ListMyInfrastructureConfigsQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value[0].ResourceGroupCount.Should().Be(3);
        result.Value[0].ResourceCount.Should().Be(10);
    }
}
