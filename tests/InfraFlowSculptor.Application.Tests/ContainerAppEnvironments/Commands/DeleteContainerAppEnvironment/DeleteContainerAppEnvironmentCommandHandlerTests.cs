using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.DeleteContainerAppEnvironment;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppEnvironmentAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.ContainerAppEnvironments.Commands.DeleteContainerAppEnvironment;

public sealed class DeleteContainerAppEnvironmentCommandHandlerTests
{
    private readonly IContainerAppEnvironmentRepository _environmentRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly ContainerAppEnvironment _environment;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly DeleteContainerAppEnvironmentCommand _command;
    private readonly DeleteContainerAppEnvironmentCommandHandler _sut;

    public DeleteContainerAppEnvironmentCommandHandlerTests()
    {
        _environmentRepository = Substitute.For<IContainerAppEnvironmentRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _environment = ContainerAppEnvironment.Create(
            _resourceGroup.Id,
            new Name("cae-shared"),
            new Location(Location.LocationEnum.FranceCentral));
        _command = new DeleteContainerAppEnvironmentCommand(_environment.Id);
        _sut = new DeleteContainerAppEnvironmentCommandHandler(
            _environmentRepository, _resourceGroupRepository, _accessService);
    }

    [Fact]
    public async Task Given_EnvironmentNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _environmentRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((ContainerAppEnvironment?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _environmentRepository.DidNotReceive().DeleteAsync(Arg.Any<AzureResourceId>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_DeletesEnvironmentAsync()
    {
        // Arrange
        _environmentRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_environment);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        await _environmentRepository.Received(1).DeleteAsync(_environment.Id);
    }
}
