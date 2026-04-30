using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.WebApps.Common;
using InfraFlowSculptor.Application.WebApps.Queries;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
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

namespace InfraFlowSculptor.Application.Tests.WebApps.Queries;

public sealed class GetWebAppQueryHandlerTests
{
    private readonly IWebAppRepository _webAppRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly WebApp _webApp;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly GetWebAppQuery _query;
    private readonly GetWebAppQueryHandler _sut;

    public GetWebAppQueryHandlerTests()
    {
        _webAppRepository = Substitute.For<IWebAppRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _webApp = WebApp.Create(
            _resourceGroup.Id,
            new Name("web-shared"),
            new Location(Location.LocationEnum.FranceCentral),
            AzureResourceId.CreateUnique(),
            new WebAppRuntimeStack(WebAppRuntimeStack.WebAppRuntimeStackEnum.DotNet),
            "8.0",
            alwaysOn: true,
            httpsOnly: true,
            new DeploymentMode(DeploymentMode.DeploymentModeType.Code),
            containerRegistryId: null,
            acrAuthMode: null,
            dockerImageName: null);
        _query = new GetWebAppQuery(_webApp.Id);
        _sut = new GetWebAppQueryHandler(
            _webAppRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_WebAppNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _webAppRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((WebApp?)null);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_ReadAccessGranted_When_Handle_Then_MapsResultAsync()
    {
        // Arrange
        _webAppRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_webApp);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _mapper.Received(1).Map<WebAppResult>(_webApp);
    }
}
