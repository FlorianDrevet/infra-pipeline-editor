using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.WebApps.Commands.CreateWebApp;
using InfraFlowSculptor.Application.WebApps.Common;
using InfraFlowSculptor.Domain.AppServicePlanAggregate;
using InfraFlowSculptor.Domain.AppServicePlanAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.WebApps.Commands.CreateWebApp;

public sealed class CreateWebAppCommandHandlerTests
{
    private const string WebAppName = "web-shared";

    private readonly IWebAppRepository _webAppRepository;
    private readonly IAppServicePlanRepository _appServicePlanRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly AppServicePlan _appServicePlan;
    private readonly CreateWebAppCommand _command;
    private readonly CreateWebAppCommandHandler _sut;

    public CreateWebAppCommandHandlerTests()
    {
        _webAppRepository = Substitute.For<IWebAppRepository>();
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
        _command = new CreateWebAppCommand(
            _resourceGroup.Id,
            new Name(WebAppName),
            new Location(Location.LocationEnum.FranceCentral),
            AppServicePlanId: _appServicePlan.Id.Value,
            RuntimeStack: nameof(WebAppRuntimeStack.WebAppRuntimeStackEnum.DotNet),
            RuntimeVersion: "8.0",
            AlwaysOn: true,
            HttpsOnly: true,
            DeploymentMode: nameof(DeploymentMode.DeploymentModeType.Code),
            ContainerRegistryId: null,
            AcrAuthMode: null,
            DockerImageName: null);
        _webAppRepository.AddAsync(Arg.Any<WebApp>())
            .Returns(callInfo => Task.FromResult((WebApp)callInfo.Args()[0]));
        _sut = new CreateWebAppCommandHandler(
            _webAppRepository, _appServicePlanRepository, _resourceGroupRepository, _accessService, _mapper);
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
        await _webAppRepository.DidNotReceive().AddAsync(Arg.Any<WebApp>());
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
        await _webAppRepository.DidNotReceive().AddAsync(Arg.Any<WebApp>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsWebAppAndMapsResultAsync()
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
        await _webAppRepository.Received(1).AddAsync(Arg.Is<WebApp>(w =>
            w.ResourceGroupId == _resourceGroup.Id
            && w.Name.Value == WebAppName
            && w.AppServicePlanId == _appServicePlan.Id));
        _mapper.Received(1).Map<WebAppResult>(Arg.Any<WebApp>());
    }
}
