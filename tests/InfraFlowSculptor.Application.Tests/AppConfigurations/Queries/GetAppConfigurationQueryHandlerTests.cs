using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.AppConfigurations.Common;
using InfraFlowSculptor.Application.AppConfigurations.Queries;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.AppConfigurationAggregate;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.AppConfigurations.Queries;

public sealed class GetAppConfigurationQueryHandlerTests
{
    private readonly IAppConfigurationRepository _appConfigurationRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly AppConfiguration _appConfiguration;
    private readonly GetAppConfigurationQuery _query;
    private readonly GetAppConfigurationQueryHandler _sut;

    public GetAppConfigurationQueryHandlerTests()
    {
        _appConfigurationRepository = Substitute.For<IAppConfigurationRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();

        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-config"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _appConfiguration = AppConfiguration.Create(
            _resourceGroup.Id,
            new Name("appcs-shared"),
            new Location(Location.LocationEnum.FranceCentral));
        _query = new GetAppConfigurationQuery(_appConfiguration.Id);
        _sut = new GetAppConfigurationQueryHandler(
            _appConfigurationRepository,
            _resourceGroupRepository,
            _accessService,
            _mapper);
    }

    [Fact]
    public async Task Given_ReadAccessDenied_When_Handle_Then_ReturnsNotFoundToHideExistenceAsync()
    {
        // Arrange
        _appConfigurationRepository.GetByIdReadOnlyAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_appConfiguration);
        _resourceGroupRepository.GetByIdReadOnlyAsync(_appConfiguration.ResourceGroupId, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Errors.InfrastructureConfig.ForbiddenError());

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _appConfigurationRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
        await _resourceGroupRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ReadAccessGranted_When_Handle_Then_MapsResultUsingReadOnlyLookupsAsync()
    {
        // Arrange
        _appConfigurationRepository.GetByIdReadOnlyAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_appConfiguration);
        _resourceGroupRepository.GetByIdReadOnlyAsync(_appConfiguration.ResourceGroupId, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _mapper.Received(1).Map<AppConfigurationResult>(_appConfiguration);
        await _appConfigurationRepository.Received(1)
            .GetByIdReadOnlyAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
        await _resourceGroupRepository.Received(1)
            .GetByIdReadOnlyAsync(_appConfiguration.ResourceGroupId, Arg.Any<CancellationToken>());
        await _appConfigurationRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
        await _resourceGroupRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
    }
}