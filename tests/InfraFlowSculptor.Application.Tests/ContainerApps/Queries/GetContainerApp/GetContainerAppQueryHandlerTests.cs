using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.ContainerApps;
using InfraFlowSculptor.Application.ContainerApps.Common;
using InfraFlowSculptor.Application.ContainerApps.Queries.GetContainerApp;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.ContainerApps.Queries.GetContainerApp;

public sealed class GetContainerAppQueryHandlerTests
{
    private readonly IContainerAppReadRepository _containerAppReadRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly ContainerApp _containerApp;
    private readonly ContainerAppResult _containerAppResult;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly GetContainerAppQuery _query;
    private readonly GetContainerAppQueryHandler _sut;

    public GetContainerAppQueryHandlerTests()
    {
        _containerAppReadRepository = Substitute.For<IContainerAppReadRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _containerApp = ContainerApp.Create(
            _resourceGroup.Id,
            new Name("ca-shared"),
            new Location(Location.LocationEnum.FranceCentral),
            AzureResourceId.CreateUnique(),
            containerRegistryId: null,
            acrAuthMode: null);
        _containerAppResult = new ContainerAppResult(
            _containerApp.Id,
            _resourceGroup.Id,
            _containerApp.Name,
            _containerApp.Location,
            _containerApp.ContainerAppEnvironmentId.Value,
            ContainerRegistryId: null,
            AcrAuthMode: null,
            AcrPullIdentityId: null,
            DockerImageName: null,
            DockerImageValidated: false,
            DockerfilePath: null,
            ApplicationName: null,
            SourceCodePath: null,
            PipelineStepOptions: null,
            EnvironmentSettings: [],
            IsExisting: false);
        _query = new GetContainerAppQuery(_containerApp.Id);
        _sut = new GetContainerAppQueryHandler(
            _containerAppReadRepository, _accessService);
    }

    [Fact]
    public async Task Given_ContainerAppNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _containerAppReadRepository.GetByIdAsync(_query.Id, Arg.Any<CancellationToken>())
            .Returns((ContainerAppDetailReadResult?)null);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _accessService.DidNotReceive()
            .VerifyReadAccessAsync(Arg.Any<Domain.InfrastructureConfigAggregate.ValueObjects.InfrastructureConfigId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ReadAccessGranted_When_Handle_Then_ReturnsProjectedResultAsync()
    {
        // Arrange
        _containerAppReadRepository.GetByIdAsync(_query.Id, Arg.Any<CancellationToken>())
            .Returns(new ContainerAppDetailReadResult(_containerAppResult, _config.Id));
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(_containerAppResult);
    }

    [Fact]
    public async Task Given_ReadAccessDenied_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _containerAppReadRepository.GetByIdAsync(_query.Id, Arg.Any<CancellationToken>())
            .Returns(new ContainerAppDetailReadResult(_containerAppResult, _config.Id));
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Errors.InfrastructureConfig.NotFoundError(_config.Id));

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }
}
