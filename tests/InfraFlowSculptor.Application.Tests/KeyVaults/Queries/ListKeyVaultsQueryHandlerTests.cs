using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.KeyVaults.Queries;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.KeyVaults.Queries;

public sealed class ListKeyVaultsQueryHandlerTests
{
    private readonly IKeyVaultRepository _keyVaultRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly ListKeyVaultsQueryHandler _sut;

    public ListKeyVaultsQueryHandlerTests()
    {
        _keyVaultRepository = Substitute.For<IKeyVaultRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _sut = new ListKeyVaultsQueryHandler(
            _keyVaultRepository,
            _resourceGroupRepository,
            _accessService,
            _mapper);
    }

    [Fact]
    public async Task Given_ResourceGroupMissing_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        var query = new ListKeyVaultsQuery(_resourceGroup.Id);
        _resourceGroupRepository.GetByIdReadOnlyAsync(query.ResourceGroupId, Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Code.Should().Be(Errors.ResourceGroup.NotFound(query.ResourceGroupId).Code);
        await _resourceGroupRepository.Received(1)
            .GetByIdReadOnlyAsync(query.ResourceGroupId, Arg.Any<CancellationToken>());
        await _resourceGroupRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ReadAccessDenied_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        var query = new ListKeyVaultsQuery(_resourceGroup.Id);
        _resourceGroupRepository.GetByIdReadOnlyAsync(query.ResourceGroupId, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Errors.InfrastructureConfig.ForbiddenError());

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Code.Should().Be(Errors.ResourceGroup.NotFound(query.ResourceGroupId).Code);
        await _resourceGroupRepository.Received(1)
            .GetByIdReadOnlyAsync(query.ResourceGroupId, Arg.Any<CancellationToken>());
        await _resourceGroupRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ReadAccessGranted_When_Handle_Then_UsesReadOnlyLookupAsync()
    {
        // Arrange
        var query = new ListKeyVaultsQuery(_resourceGroup.Id);
        _resourceGroupRepository.GetByIdReadOnlyAsync(query.ResourceGroupId, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _keyVaultRepository.GetByResourceGroupIdAsync(query.ResourceGroupId, Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
        await _resourceGroupRepository.Received(1)
            .GetByIdReadOnlyAsync(query.ResourceGroupId, Arg.Any<CancellationToken>());
        await _resourceGroupRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
    }
}
