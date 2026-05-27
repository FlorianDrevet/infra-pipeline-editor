using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Queries.ListProjectPipelineVariableGroups;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Queries.ListProjectPipelineVariableGroups;

public sealed class ListProjectPipelineVariableGroupsQueryHandlerTests
{
    private readonly IProjectAccessService _accessService;
    private readonly IProjectRepository _projectRepository;
    private readonly Project _project;
    private readonly ProjectId _projectId;
    private readonly ListProjectPipelineVariableGroupsQueryHandler _sut;

    public ListProjectPipelineVariableGroupsQueryHandlerTests()
    {
        _accessService = Substitute.For<IProjectAccessService>();
        _projectRepository = Substitute.For<IProjectRepository>();
        _project = Project.Create(new Name("TestProject"), null, UserId.CreateUnique());
        _projectId = _project.Id;
        _sut = new ListProjectPipelineVariableGroupsQueryHandler(_accessService, _projectRepository);
    }

    [Fact]
    public async Task Given_AccessGrantedAndGroupsExist_When_Handle_Then_ReturnsGroupListAsync()
    {
        // Arrange
        _project.AddPipelineVariableGroup("MyApp-Secrets");
        var group = _project.PipelineVariableGroups.First();
        var usages = new Dictionary<Guid, List<PipelineVariableUsageResult>>
        {
            {
                group.Id.Value,
                [new PipelineVariableUsageResult("DB_CONNECTION", "ConnectionString", "SqlServer", "SqlServer", "Dev")]
            }
        };

        _accessService.VerifyReadAccessAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithPipelineVariableGroupsAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetPipelineVariableUsagesAsync(
                Arg.Any<List<ProjectPipelineVariableGroupId>>(), Arg.Any<CancellationToken>())
            .Returns(usages);
        var query = new ListProjectPipelineVariableGroupsQuery(_projectId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
        result.Value[0].GroupName.Should().Be("MyApp-Secrets");
        result.Value[0].Variables.Should().HaveCount(1);
        result.Value[0].Variables[0].PipelineVariableName.Should().Be("DB_CONNECTION");
    }

    [Fact]
    public async Task Given_ReadAccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        _accessService.VerifyReadAccessAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(Errors.Project.NotFoundError(_projectId));
        var query = new ListProjectPipelineVariableGroupsQuery(_projectId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _projectRepository.DidNotReceive()
            .GetByIdWithPipelineVariableGroupsAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_AccessGrantedButProjectNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _accessService.VerifyReadAccessAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithPipelineVariableGroupsAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns((Project?)null);
        var query = new ListProjectPipelineVariableGroupsQuery(_projectId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }
}
