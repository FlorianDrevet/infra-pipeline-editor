using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.DownloadBicep;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.DownloadBicep;

public sealed class DownloadBicepCommandHandlerTests
{
    private readonly IBlobService _blobService;
    private readonly IInfraConfigAccessService _accessService;
    private readonly DownloadBicepCommandHandler _sut;

    private readonly Guid _configId = Guid.NewGuid();
    private readonly DomainInfrastructureConfig _config;

    public DownloadBicepCommandHandlerTests()
    {
        _blobService = Substitute.For<IBlobService>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _config = DomainInfrastructureConfig.Create(new Name("test-config"), ProjectId.CreateUnique());
        _sut = new DownloadBicepCommandHandler(_blobService, _accessService);
    }

    [Fact]
    public async Task Given_ReadAccessGrantedAndBlobsExist_When_Handle_Then_ReturnsZipResultAsync()
    {
        // Arrange
        var command = new DownloadBicepCommand(_configId);
        _accessService.VerifyReadAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(_config);

        var blobName = $"bicep/{_configId}/20260517120000/main.bicep";
        _blobService.ListBlobsAsync($"bicep/{_configId}/")
            .Returns(new List<string> { blobName });
        _blobService.DownloadContentAsync(blobName)
            .Returns("param location string");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ZipContent.Should().NotBeEmpty();
        result.Value.FileName.Should().Contain("bicep");
    }

    [Fact]
    public async Task Given_ReadAccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        var command = new DownloadBicepCommand(_configId);
        _accessService.VerifyReadAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(Errors.InfrastructureConfig.NotFoundError(new InfrastructureConfigId(_configId)));

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
        var command = new DownloadBicepCommand(_configId);
        _accessService.VerifyReadAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(_config);
        _blobService.ListBlobsAsync($"bicep/{_configId}/")
            .Returns(new List<string>());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
    }
}
