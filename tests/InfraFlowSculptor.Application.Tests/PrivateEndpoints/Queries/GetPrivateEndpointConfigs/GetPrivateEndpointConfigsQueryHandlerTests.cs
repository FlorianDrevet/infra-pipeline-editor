using System.Reflection;
using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.PrivateEndpoints.Common;
using InfraFlowSculptor.Application.PrivateEndpoints.Queries.GetPrivateEndpointConfigs;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.PrivateEndpoints.Queries.GetPrivateEndpointConfigs;

public sealed class GetPrivateEndpointConfigsQueryHandlerTests
{
    private readonly IAzureResourceRepository _resourceRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly GetPrivateEndpointConfigsQueryHandler _sut;

    public GetPrivateEndpointConfigsQueryHandlerTests()
    {
        _resourceRepository = Substitute.For<IAzureResourceRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _sut = new GetPrivateEndpointConfigsQueryHandler(_resourceRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_ResourceNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        var resourceId = AzureResourceId.CreateUnique();
        var query = new GetPrivateEndpointConfigsQuery(resourceId);
        _resourceRepository.GetByIdWithPrivateEndpointsReadOnlyAsync(resourceId, Arg.Any<CancellationToken>())
            .Returns((AzureResource?)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        var resource = CreateResourceWithResourceGroup();
        var query = new GetPrivateEndpointConfigsQuery(resource.Id);

        _resourceRepository.GetByIdWithPrivateEndpointsReadOnlyAsync(resource.Id, Arg.Any<CancellationToken>())
            .Returns(resource);
        _accessService.VerifyReadAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(Error.Forbidden());

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Given_ResourceWithNoPrivateEndpoints_When_Handle_Then_ReturnsEmptyListAsync()
    {
        // Arrange
        var resource = CreateResourceWithResourceGroup();
        var query = new GetPrivateEndpointConfigsQuery(resource.Id);

        _resourceRepository.GetByIdWithPrivateEndpointsReadOnlyAsync(resource.Id, Arg.Any<CancellationToken>())
            .Returns(resource);
        _accessService.VerifyReadAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }

    private AzureResource CreateResourceWithResourceGroup()
    {
        var resource = KeyVault.Create(
            _resourceGroup.Id,
            new Name("kv-shared"),
            new Location(Location.LocationEnum.FranceCentral));

        // Set the protected ResourceGroup navigation property via reflection
        // to simulate what EF Core does when loading with Include().
        typeof(AzureResource)
            .GetProperty(nameof(AzureResource.ResourceGroup))!
            .SetValue(resource, _resourceGroup);

        return resource;
    }
}
