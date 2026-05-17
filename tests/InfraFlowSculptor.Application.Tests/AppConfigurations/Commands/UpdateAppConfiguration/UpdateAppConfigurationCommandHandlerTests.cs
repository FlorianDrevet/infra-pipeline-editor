using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.AppConfigurations.Commands.UpdateAppConfiguration;
using InfraFlowSculptor.Application.AppConfigurations.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.AppConfigurationAggregate;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.AppConfigurations.Commands.UpdateAppConfiguration;

public sealed class UpdateAppConfigurationCommandHandlerTests
{
    private readonly IAppConfigurationRepository _appConfigurationRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly AppConfiguration _existingEntity;
    private readonly UpdateAppConfigurationCommand _command;
    private readonly UpdateAppConfigurationCommandHandler _sut;

    public UpdateAppConfigurationCommandHandlerTests()
    {
        _appConfigurationRepository = Substitute.For<IAppConfigurationRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _existingEntity = AppConfiguration.Create(
            _resourceGroup.Id,
            new Name("appconfig-old"),
            new Location(Location.LocationEnum.FranceCentral));
        _command = new UpdateAppConfigurationCommand(
            _existingEntity.Id,
            new Name("appconfig-renamed"),
            new Location(Location.LocationEnum.WestEurope));
        _appConfigurationRepository.Update(Arg.Any<AppConfiguration>())
            .Returns(callInfo => (AppConfiguration)callInfo.Args()[0]);
        _sut = new UpdateAppConfigurationCommandHandler(
            _appConfigurationRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_EntityNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _appConfigurationRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((AppConfiguration?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _appConfigurationRepository.DidNotReceive().Update(Arg.Any<AppConfiguration>());
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _appConfigurationRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _appConfigurationRepository.DidNotReceive().Update(Arg.Any<AppConfiguration>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsUpdatedEntityAndMapsResultAsync()
    {
        // Arrange
        _appConfigurationRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _appConfigurationRepository.Received(1).Update(Arg.Is<AppConfiguration>(e =>
            e.Name.Value == "appconfig-renamed"));
        _mapper.Received(1).Map<AppConfigurationResult>(Arg.Any<AppConfiguration>());
    }
}
