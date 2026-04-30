using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.FunctionApps.Commands.CreateFunctionApp;
using InfraFlowSculptor.Application.FunctionApps.Common;
using InfraFlowSculptor.Domain.AppServicePlanAggregate;
using InfraFlowSculptor.Domain.AppServicePlanAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.FunctionAppAggregate;
using InfraFlowSculptor.Domain.FunctionAppAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.FunctionApps.Commands.CreateFunctionApp;

public sealed class CreateFunctionAppCommandHandlerTests
{
    private const string FunctionAppName = "func-shared";

    private readonly IFunctionAppRepository _functionAppRepository;
    private readonly IAppServicePlanRepository _appServicePlanRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly AppServicePlan _appServicePlan;
    private readonly CreateFunctionAppCommand _command;
    private readonly CreateFunctionAppCommandHandler _sut;

    public CreateFunctionAppCommandHandlerTests()
    {
        _functionAppRepository = Substitute.For<IFunctionAppRepository>();
        _appServicePlanRepository = Substitute.For<IAppServicePlanRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _appServicePlan = AppServicePlan.Create(
            _resourceGroup.Id,
            new Name("asp-shared"),
            new Location(Location.LocationEnum.FranceCentral),
            new AppServicePlanOsType(AppServicePlanOsType.AppServicePlanOsTypeEnum.Linux));
        _command = new CreateFunctionAppCommand(
            _resourceGroup.Id,
            new Name(FunctionAppName),
            new Location(Location.LocationEnum.FranceCentral),
            AppServicePlanId: _appServicePlan.Id.Value,
            RuntimeStack: nameof(FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum.DotNet),
            RuntimeVersion: "8.0",
            HttpsOnly: true,
            DeploymentMode: nameof(DeploymentMode.DeploymentModeType.Code),
            ContainerRegistryId: null,
            AcrAuthMode: null,
            DockerImageName: null);
        _functionAppRepository.AddAsync(Arg.Any<FunctionApp>())
            .Returns(callInfo => Task.FromResult((FunctionApp)callInfo.Args()[0]));
        _sut = new CreateFunctionAppCommandHandler(
            _functionAppRepository, _appServicePlanRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _functionAppRepository.DidNotReceive().AddAsync(Arg.Any<FunctionApp>());
    }

    [Fact]
    public async Task Given_AppServicePlanNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _appServicePlanRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((AppServicePlan?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _functionAppRepository.DidNotReceive().AddAsync(Arg.Any<FunctionApp>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsFunctionAppAndMapsResultAsync()
    {
        // Arrange
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _appServicePlanRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_appServicePlan);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        await _functionAppRepository.Received(1).AddAsync(Arg.Is<FunctionApp>(f =>
            f.ResourceGroupId == _resourceGroup.Id
            && f.Name.Value == FunctionAppName
            && f.AppServicePlanId == _appServicePlan.Id));
        _mapper.Received(1).Map<FunctionAppResult>(Arg.Any<FunctionApp>());
    }
}
