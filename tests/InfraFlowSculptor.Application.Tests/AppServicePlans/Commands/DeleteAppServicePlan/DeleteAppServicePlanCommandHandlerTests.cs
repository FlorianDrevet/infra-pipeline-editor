using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.AppServicePlans.Commands.DeleteAppServicePlan;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.AppServicePlanAggregate;
using InfraFlowSculptor.Domain.AppServicePlanAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.FunctionAppAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.AppServicePlans.Commands.DeleteAppServicePlan;

public sealed class DeleteAppServicePlanCommandHandlerTests
{
    private readonly IAppServicePlanRepository _appServicePlanRepository;
    private readonly IWebAppRepository _webAppRepository;
    private readonly IFunctionAppRepository _functionAppRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly AppServicePlan _plan;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly DeleteAppServicePlanCommand _command;
    private readonly DeleteAppServicePlanCommandHandler _sut;

    public DeleteAppServicePlanCommandHandlerTests()
    {
        _appServicePlanRepository = Substitute.For<IAppServicePlanRepository>();
        _webAppRepository = Substitute.For<IWebAppRepository>();
        _functionAppRepository = Substitute.For<IFunctionAppRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _plan = AppServicePlan.Create(
            _resourceGroup.Id,
            new Name("asp-shared"),
            new Location(Location.LocationEnum.FranceCentral),
            new AppServicePlanOsType(AppServicePlanOsType.AppServicePlanOsTypeEnum.Linux));
        _command = new DeleteAppServicePlanCommand(_plan.Id);
        _webAppRepository.GetByAppServicePlanIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(new List<WebApp>());
        _functionAppRepository.GetByAppServicePlanIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(new List<FunctionApp>());
        _sut = new DeleteAppServicePlanCommandHandler(
            _appServicePlanRepository,
            _webAppRepository,
            _functionAppRepository,
            _resourceGroupRepository,
            _accessService);
    }

    [Fact]
    public async Task Given_AppServicePlanNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _appServicePlanRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((AppServicePlan?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _appServicePlanRepository.DidNotReceive().DeleteAsync(Arg.Any<AzureResourceId>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_DeletesAppServicePlanAsync()
    {
        // Arrange
        _appServicePlanRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_plan);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        await _appServicePlanRepository.Received(1).DeleteAsync(_plan.Id);
    }
}
