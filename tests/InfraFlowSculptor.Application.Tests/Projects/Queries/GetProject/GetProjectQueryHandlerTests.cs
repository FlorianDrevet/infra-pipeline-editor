using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Queries.GetProject;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Queries.GetProject;

public sealed class GetProjectQueryHandlerTests
{
    private const string ProjectName = "RetailApi";
    private const string FirstResourceType = "StorageAccount";
    private const string SecondResourceType = "KeyVault";

    private readonly IProjectAccessService _accessService;
    private readonly IProjectRepository _projectRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly Project _project;
    private readonly ProjectId _projectId;
    private readonly GetProjectQueryHandler _sut;

    public GetProjectQueryHandlerTests()
    {
        _accessService = Substitute.For<IProjectAccessService>();
        _projectRepository = Substitute.For<IProjectRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _project = Project.Create(new Name(ProjectName), null, UserId.CreateUnique());
        _projectId = _project.Id;
        _sut = new GetProjectQueryHandler(_accessService, _projectRepository, _resourceGroupRepository);
    }

    [Fact]
    public async Task Given_ReadAccessGrantedAndProjectExists_When_Handle_Then_ReturnsProjectWithUsedResourceTypesAsync()
    {
        // Arrange
        var expectedResourceTypes = new List<string> { FirstResourceType, SecondResourceType };
        _accessService.VerifyReadAccessAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(_project);
        _resourceGroupRepository.GetDistinctResourceTypesByProjectIdAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(expectedResourceTypes);
        var query = new GetProjectQuery(_projectId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Id.Should().Be(_projectId);
        result.Value.Name.Value.Should().Be(ProjectName);
        result.Value.UsedResourceTypes.Should().BeEquivalentTo(expectedResourceTypes);
    }

    [Fact]
    public async Task Given_ReadAccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        _accessService.VerifyReadAccessAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(Errors.Project.NotFoundError(_projectId));
        var query = new GetProjectQuery(_projectId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _projectRepository.DidNotReceive().GetByIdWithAllAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_AccessGrantedButReloadReturnsNull_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _accessService.VerifyReadAccessAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns((Project?)null);
        var query = new GetProjectQuery(_projectId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }
}
