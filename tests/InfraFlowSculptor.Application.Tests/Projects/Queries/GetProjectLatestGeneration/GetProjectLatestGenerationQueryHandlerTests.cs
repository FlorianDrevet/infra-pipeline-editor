using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Queries.GetProjectLatestGeneration;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Queries.GetProjectLatestGeneration;

public sealed class GetProjectLatestGenerationQueryHandlerTests
{
    private const string ProjectName = "RetailApi";

    private readonly IProjectAccessService _accessService;
    private readonly IBlobService _blobService;
    private readonly Guid _projectId;
    private readonly Project _project;
    private readonly GetProjectLatestGenerationQueryHandler _sut;

    public GetProjectLatestGenerationQueryHandlerTests()
    {
        _accessService = Substitute.For<IProjectAccessService>();
        _blobService = Substitute.For<IBlobService>();
        _projectId = Guid.NewGuid();
        _project = Project.Create(new Name(ProjectName), null, UserId.CreateUnique());
        _sut = new GetProjectLatestGenerationQueryHandler(_accessService, _blobService);
    }

    [Fact]
    public async Task Given_LegacyAndSplitPipelineArtifacts_When_Handle_Then_ReturnsNormalizedPipelineFilesAndGeneratedAtAsync()
    {
        // Arrange
        _accessService.VerifyReadAccessAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns(_project);
        _blobService.ListBlobsAsync($"bicep/project/{_projectId}/")
            .Returns([]);
        _blobService.ListBlobsAsync($"pipeline/project/{_projectId}/")
            .Returns(
            [
                $"pipeline/project/{_projectId}/20260512090000/.azuredevops/Common/build.yml",
                $"pipeline/project/{_projectId}/20260512090000/.azuredevops/dev/apps/api/api/ci.yml",
                $"pipeline/project/{_projectId}/20260512090000/app/.azuredevops/prod/apps/web/web/release.yml",
                $"pipeline/project/{_projectId}/20260511090000/.azuredevops/Common/old.yml",
            ]);
        _blobService.ListBlobsAsync($"bootstrap/project/{_projectId}/")
            .Returns([]);

        var query = new GetProjectLatestGenerationQuery(_projectId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.GeneratedAt.Should().Be("20260512090000");
        result.Value.Bicep.Should().BeNull();
        result.Value.Bootstrap.Should().BeNull();
        result.Value.Pipeline.Should().NotBeNull();
        result.Value.Pipeline!.CommonFilePaths.Should().ContainKey(".azuredevops/Common/build.yml");
        result.Value.Pipeline.InfraCommonFilePaths.Should().ContainKey(".azuredevops/Common/build.yml");
        result.Value.Pipeline.ConfigFilePaths.Should().ContainKey("dev");
        result.Value.Pipeline.ConfigFilePaths["dev"].Should().ContainKey("apps/api/ci.yml");
        result.Value.Pipeline.InfraConfigFilePaths.Should().ContainKey("dev");
        result.Value.Pipeline.InfraConfigFilePaths["dev"].Should().ContainKey("apps/api/ci.yml");
        result.Value.Pipeline.ConfigFilePaths.Should().ContainKey("prod");
        result.Value.Pipeline.ConfigFilePaths["prod"].Should().ContainKey("apps/web/release.yml");
        result.Value.Pipeline.AppConfigFilePaths.Should().ContainKey("prod");
        result.Value.Pipeline.AppConfigFilePaths["prod"].Should().ContainKey("apps/web/release.yml");
    }

    [Fact]
    public async Task Given_ReadAccessDenied_When_Handle_Then_ReturnsErrorWithoutListingBlobsAsync()
    {
        // Arrange
        _accessService.VerifyReadAccessAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns(Error.NotFound(code: "Project.NotFound", description: "Project was not found."));
        var query = new GetProjectLatestGenerationQuery(_projectId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _blobService.DidNotReceive().ListBlobsAsync(Arg.Any<string>());
    }
}