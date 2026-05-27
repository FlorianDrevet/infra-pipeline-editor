using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Queries.GetProjectPipelineFileContent;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Queries.GetProjectPipelineFileContent;

public sealed class GetProjectPipelineFileContentQueryHandlerTests
{
    private const string FilePath = "azure-pipelines.yml";
    private const string FileContent = "trigger:\n  - main";

    private readonly IProjectAccessService _accessService;
    private readonly IBlobService _blobService;
    private readonly Project _project;
    private readonly Guid _projectGuid;
    private readonly GetProjectPipelineFileContentQueryHandler _sut;

    public GetProjectPipelineFileContentQueryHandlerTests()
    {
        _accessService = Substitute.For<IProjectAccessService>();
        _blobService = Substitute.For<IBlobService>();
        _project = Project.Create(new Name("TestProject"), null, UserId.CreateUnique());
        _projectGuid = _project.Id.Value;
        _sut = new GetProjectPipelineFileContentQueryHandler(_accessService, _blobService);
    }

    [Fact]
    public async Task Given_ReadAccessGrantedAndFileExists_When_Handle_Then_ReturnsContentAsync()
    {
        // Arrange
        var blobPrefix = $"pipeline/project/{_projectGuid}/";
        var timestamp = "20260101120000";
        var blobPath = $"pipeline/project/{_projectGuid}/{timestamp}/{FilePath}";
        _accessService.VerifyReadAccessAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns(_project);
        _blobService.ListBlobsAsync(blobPrefix)
            .Returns(new List<string> { blobPath });
        _blobService.DownloadContentAsync(blobPath)
            .Returns(FileContent);
        var query = new GetProjectPipelineFileContentQuery(_projectGuid, FilePath);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Content.Should().Be(FileContent);
    }

    [Fact]
    public async Task Given_ReadAccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        _accessService.VerifyReadAccessAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns(Errors.Project.NotFoundError(new ProjectId(_projectGuid)));
        var query = new GetProjectPipelineFileContentQuery(_projectGuid, FilePath);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _blobService.DidNotReceive().ListBlobsAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Given_AccessGrantedButNoBlobsExist_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _accessService.VerifyReadAccessAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns(_project);
        _blobService.ListBlobsAsync(Arg.Any<string>())
            .Returns(new List<string>());
        var query = new GetProjectPipelineFileContentQuery(_projectGuid, FilePath);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }
}
