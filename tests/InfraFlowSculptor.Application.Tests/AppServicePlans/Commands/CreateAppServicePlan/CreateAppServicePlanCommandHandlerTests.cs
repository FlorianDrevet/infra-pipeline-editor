using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.AppServicePlans.Commands.CreateAppServicePlan;
using InfraFlowSculptor.Application.AppServicePlans.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.AppServicePlanAggregate;
using InfraFlowSculptor.Domain.AppServicePlanAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.AppServicePlans.Commands.CreateAppServicePlan;

public sealed class CreateAppServicePlanCommandHandlerTests
{
    private const string AppServicePlanName = "asp-shared";

    private readonly IAppServicePlanRepository _appServicePlanRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly CreateAppServicePlanCommand _command;
    private readonly CreateAppServicePlanCommandHandler _sut;

    public CreateAppServicePlanCommandHandlerTests()
    {
        _appServicePlanRepository = Substitute.For<IAppServicePlanRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _command = new CreateAppServicePlanCommand(
            _resourceGroup.Id,
            new Name(AppServicePlanName),
            new Location(Location.LocationEnum.FranceCentral),
            OsType: nameof(AppServicePlanOsType.AppServicePlanOsTypeEnum.Linux));
        _appServicePlanRepository.AddAsync(Arg.Any<AppServicePlan>())
            .Returns(callInfo => Task.FromResult((AppServicePlan)callInfo.Args()[0]));
        _sut = new CreateAppServicePlanCommandHandler(
            _appServicePlanRepository, _resourceGroupRepository, _accessService, _mapper);
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
        await _appServicePlanRepository.DidNotReceive().AddAsync(Arg.Any<AppServicePlan>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsAppServicePlanAndMapsResultAsync()
    {
        // Arrange
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        await _appServicePlanRepository.Received(1).AddAsync(Arg.Is<AppServicePlan>(p =>
            p.ResourceGroupId == _resourceGroup.Id
            && p.Name.Value == AppServicePlanName
            && p.OsType.Value == AppServicePlanOsType.AppServicePlanOsTypeEnum.Linux));
        _mapper.Received(1).Map<AppServicePlanResult>(Arg.Any<AppServicePlan>());
    }
}
