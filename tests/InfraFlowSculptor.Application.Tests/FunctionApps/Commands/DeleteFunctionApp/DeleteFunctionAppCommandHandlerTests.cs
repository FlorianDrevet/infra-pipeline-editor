using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.FunctionApps.Commands.DeleteFunctionApp;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.FunctionAppAggregate;
using InfraFlowSculptor.Domain.FunctionAppAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.FunctionApps.Commands.DeleteFunctionApp;

public sealed class DeleteFunctionAppCommandHandlerTests
{
    private readonly IFunctionAppRepository _functionAppRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly FunctionApp _functionApp;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly DeleteFunctionAppCommand _command;
    private readonly DeleteFunctionAppCommandHandler _sut;

    public DeleteFunctionAppCommandHandlerTests()
    {
        _functionAppRepository = Substitute.For<IFunctionAppRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _functionApp = FunctionApp.Create(
            _resourceGroup.Id,
            new Name("func-shared"),
            new Location(Location.LocationEnum.FranceCentral),
            AzureResourceId.CreateUnique(),
            new FunctionAppRuntimeStack(FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum.DotNet),
            "8.0",
            httpsOnly: true,
            new DeploymentMode(DeploymentMode.DeploymentModeType.Code),
            containerRegistryId: null,
            acrAuthMode: null,
            dockerImageName: null);
        _command = new DeleteFunctionAppCommand(_functionApp.Id);
        _sut = new DeleteFunctionAppCommandHandler(
            _functionAppRepository, _resourceGroupRepository, _accessService);
    }

    [Fact]
    public async Task Given_FunctionAppNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _functionAppRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((FunctionApp?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _functionAppRepository.DidNotReceive().DeleteAsync(Arg.Any<AzureResourceId>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_DeletesFunctionAppAsync()
    {
        // Arrange
        _functionAppRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_functionApp);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        await _functionAppRepository.Received(1).DeleteAsync(_functionApp.Id);
    }
}
