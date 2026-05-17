using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Commands.DownloadProjectBootstrapPipeline;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.DownloadProjectBootstrapPipeline;

public sealed class DownloadProjectBootstrapPipelineCommandHandlerTests
{
    private readonly IProjectAccessService _accessService;
    private readonly IBlobService _blobService;
    private readonly DownloadProjectBootstrapPipelineCommandHandler _sut;

    private readonly ProjectId _projectId = ProjectId.CreateUnique();
    private readonly Project _project;

    public DownloadProjectBootstrapPipelineCommandHandlerTests()
    {
        _accessService = Substitute.For<IProjectAccessService>();
        _blobService = Substitute.For<IBlobService>();
        _project = Project.Create(new Name("test-project"), null, UserId.CreateUnique());
        _sut = new DownloadProjectBootstrapPipelineCommandHandler(_accessService, _blobService);
    }

    [Fact]
    public async Task Given_ReadAccessGrantedAndBlobsExist_When_Handle_Then_ReturnsZipResultAsync()
    {
        // Arrange
        var command = new DownloadProjectBootstrapPipelineCommand(_projectId);
        _accessService.VerifyReadAccessAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(_project);

        var blobName = $"bootstrap/project/{_projectId.Value}/20260517120000/bootstrap.yml";
        _blobService.ListBlobsAsync($"bootstrap/project/{_projectId.Value}/")
            .Returns(new List<string> { blobName });
        _blobService.DownloadContentAsync(blobName)
            .Returns("trigger: none");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ZipContent.Should().NotBeEmpty();
        result.Value.FileName.Should().Contain("bootstrap-pipeline");
    }

    [Fact]
    public async Task Given_ReadAccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        var command = new DownloadProjectBootstrapPipelineCommand(_projectId);
        _accessService.VerifyReadAccessAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(Errors.Project.NotFoundError(_projectId));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _blobService.DidNotReceive().ListBlobsAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Given_NoBlobsExist_When_Handle_Then_ReturnsNotFoundErrorAsync()
    {
        // Arrange
        var command = new DownloadProjectBootstrapPipelineCommand(_projectId);
        _accessService.VerifyReadAccessAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(_project);
        _blobService.ListBlobsAsync($"bootstrap/project/{_projectId.Value}/")
            .Returns(new List<string>());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
    }
}
