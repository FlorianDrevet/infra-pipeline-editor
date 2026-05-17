using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Queries.ListProjectResources;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Queries.ListProjectResources;

public sealed class ListProjectResourcesQueryHandlerTests
{
    private readonly IProjectAccessService _projectAccessService;
    private readonly IInfrastructureConfigRepository _infraConfigRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly Project _project;
    private readonly Guid _projectGuid;
    private readonly ListProjectResourcesQueryHandler _sut;

    public ListProjectResourcesQueryHandlerTests()
    {
        _projectAccessService = Substitute.For<IProjectAccessService>();
        _infraConfigRepository = Substitute.For<IInfrastructureConfigRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _project = Project.Create(new Name("TestProject"), null, UserId.CreateUnique());
        _projectGuid = _project.Id.Value;
        _sut = new ListProjectResourcesQueryHandler(
            _projectAccessService, _infraConfigRepository, _resourceGroupRepository);
    }

    [Fact]
    public async Task Given_AccessGrantedAndNoConfigs_When_Handle_Then_ReturnsEmptyListAsync()
    {
        // Arrange
        _projectAccessService.VerifyReadAccessAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns(_project);
        _infraConfigRepository.GetByProjectIdAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns(new List<DomainInfrastructureConfig>());
        var query = new ListProjectResourcesQuery(_projectGuid);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_ReadAccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        _projectAccessService.VerifyReadAccessAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns(Errors.Project.NotFoundError(new ProjectId(_projectGuid)));
        var query = new ListProjectResourcesQuery(_projectGuid);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _infraConfigRepository.DidNotReceive()
            .GetByProjectIdAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_AccessGrantedAndConfigWithEmptyResourceGroups_When_Handle_Then_ReturnsEmptyListAsync()
    {
        // Arrange
        var config = DomainInfrastructureConfig.Create(new Name("Dev"), _project.Id);
        _projectAccessService.VerifyReadAccessAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns(_project);
        _infraConfigRepository.GetByProjectIdAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns(new List<DomainInfrastructureConfig> { config });
        _resourceGroupRepository.GetByInfraConfigIdAsync(config.Id, Arg.Any<CancellationToken>())
            .Returns(new List<DomainResourceGroup>());
        var query = new ListProjectResourcesQuery(_projectGuid);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }
}
