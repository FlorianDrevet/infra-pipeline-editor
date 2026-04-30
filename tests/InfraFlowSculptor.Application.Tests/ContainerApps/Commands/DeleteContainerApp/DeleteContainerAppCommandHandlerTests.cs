using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ContainerApps.Commands.DeleteContainerApp;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.ContainerApps.Commands.DeleteContainerApp;

public sealed class DeleteContainerAppCommandHandlerTests
{
    private readonly IContainerAppRepository _containerAppRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly ContainerApp _containerApp;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly DeleteContainerAppCommand _command;
    private readonly DeleteContainerAppCommandHandler _sut;

    public DeleteContainerAppCommandHandlerTests()
    {
        _containerAppRepository = Substitute.For<IContainerAppRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
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
        _command = new DeleteContainerAppCommand(_containerApp.Id);
        _sut = new DeleteContainerAppCommandHandler(
            _containerAppRepository, _resourceGroupRepository, _accessService);
    }

    [Fact]
    public async Task Given_ContainerAppNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _containerAppRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((ContainerApp?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _containerAppRepository.DidNotReceive().DeleteAsync(Arg.Any<AzureResourceId>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_DeletesContainerAppAsync()
    {
        // Arrange
        _containerAppRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_containerApp);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        await _containerAppRepository.Received(1).DeleteAsync(_containerApp.Id);
    }
}
