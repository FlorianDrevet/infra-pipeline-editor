using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.DownloadBootstrap;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.DownloadBootstrap;

public sealed class DownloadBootstrapCommandHandlerTests
{
    private readonly IGeneratedArtifactService _artifactService;
    private readonly IInfraConfigAccessService _accessService;
    private readonly DownloadBootstrapCommandHandler _sut;

    public DownloadBootstrapCommandHandlerTests()
    {
        _artifactService = Substitute.For<IGeneratedArtifactService>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _sut = new DownloadBootstrapCommandHandler(_artifactService, _accessService);
    }

    [Fact]
    public async Task Given_ReadAccessDenied_When_Handle_Then_ReturnsErrorAndDoesNotDownloadAsync()
    {
        // Arrange
        var command = new DownloadBootstrapCommand(Guid.NewGuid());
        _accessService.VerifyReadAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(Errors.InfrastructureConfig.ForbiddenError());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        await _artifactService.DidNotReceive()
            .DownloadLatestAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ReadAccessGranted_When_Handle_Then_DownloadsLatestBootstrapArchiveAsync()
    {
        // Arrange
        var command = new DownloadBootstrapCommand(Guid.NewGuid());
        var zipContent = new byte[] { 1, 2, 3 };
        const string fileName = "bootstrap.zip";
        var config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _accessService.VerifyReadAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(config);
        _artifactService.DownloadLatestAsync("bootstrap", command.InfrastructureConfigId, Arg.Any<CancellationToken>())
            .Returns((zipContent, fileName));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ZipContent.Should().Equal(zipContent);
        result.Value.FileName.Should().Be(fileName);
        await _artifactService.Received(1)
            .DownloadLatestAsync("bootstrap", command.InfrastructureConfigId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_NoArtifactsGenerated_When_Handle_Then_ReturnsNotFoundErrorAsync()
    {
        // Arrange
        var command = new DownloadBootstrapCommand(Guid.NewGuid());
        var config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _accessService.VerifyReadAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(config);
        _artifactService.DownloadLatestAsync("bootstrap", command.InfrastructureConfigId, Arg.Any<CancellationToken>())
            .Returns(((byte[] ZipContent, string FileName)?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }
}
