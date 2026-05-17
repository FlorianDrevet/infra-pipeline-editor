using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.Projects.Queries.ListProjectConfigs;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Queries.ListProjectConfigs;

public sealed class ListProjectConfigsQueryHandlerTests
{
    private readonly IProjectAccessService _accessService;
    private readonly IInfrastructureConfigRepository _configRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IMapper _mapper;
    private readonly Project _project;
    private readonly ProjectId _projectId;
    private readonly ListProjectConfigsQueryHandler _sut;

    public ListProjectConfigsQueryHandlerTests()
    {
        _accessService = Substitute.For<IProjectAccessService>();
        _configRepository = Substitute.For<IInfrastructureConfigRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _mapper = Substitute.For<IMapper>();
        _project = Project.Create(new Name("TestProject"), null, UserId.CreateUnique());
        _projectId = _project.Id;
        _sut = new ListProjectConfigsQueryHandler(
            _accessService, _configRepository, _resourceGroupRepository, _mapper);
    }

    [Fact]
    public async Task Given_ReadAccessGrantedAndConfigsExist_When_Handle_Then_ReturnsConfigListAsync()
    {
        // Arrange
        var configId = InfrastructureConfigId.CreateUnique();
        var config = DomainInfrastructureConfig.Create(new Name("Dev"), _projectId);
        var configResult = new GetInfrastructureConfigResult(
            configId, new Name("Dev"), _projectId, null, false, [], [], [], 0, 0, 0, null, null);
        var counts = new Dictionary<Guid, (int ResourceGroupCount, int ResourceCount)>
        {
            { configResult.Id.Value, (2, 5) }
        };

        _accessService.VerifyReadAccessAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(_project);
        _configRepository.GetByProjectIdAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(new List<DomainInfrastructureConfig> { config });
        _mapper.Map<GetInfrastructureConfigResult>(config)
            .Returns(configResult);
        _resourceGroupRepository.GetResourceCountsByInfraConfigIdsAsync(
                Arg.Any<List<InfrastructureConfigId>>(), Arg.Any<CancellationToken>())
            .Returns(counts);
        var query = new ListProjectConfigsQuery(_projectId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
        result.Value[0].ResourceGroupCount.Should().Be(2);
        result.Value[0].ResourceCount.Should().Be(5);
    }

    [Fact]
    public async Task Given_ReadAccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        _accessService.VerifyReadAccessAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(Errors.Project.NotFoundError(_projectId));
        var query = new ListProjectConfigsQuery(_projectId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _configRepository.DidNotReceive()
            .GetByProjectIdAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_AccessGrantedButNoConfigs_When_Handle_Then_ReturnsEmptyListAsync()
    {
        // Arrange
        _accessService.VerifyReadAccessAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(_project);
        _configRepository.GetByProjectIdAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(new List<DomainInfrastructureConfig>());
        _resourceGroupRepository.GetResourceCountsByInfraConfigIdsAsync(
                Arg.Any<List<InfrastructureConfigId>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, (int ResourceGroupCount, int ResourceCount)>());
        var query = new ListProjectConfigsQuery(_projectId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }
}
