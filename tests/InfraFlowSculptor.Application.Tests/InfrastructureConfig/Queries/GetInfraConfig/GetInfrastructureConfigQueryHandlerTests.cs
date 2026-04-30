using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetInfraConfig;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Queries.GetInfraConfig;

public sealed class GetInfrastructureConfigQueryHandlerTests
{
    private const string ConfigName = "primary";
    private const int ExpectedResourceGroupCount = 3;
    private const int ExpectedResourceCount = 12;

    private readonly IInfraConfigAccessService _accessService;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IMapper _mapper;
    private readonly DomainInfrastructureConfig _config;
    private readonly InfrastructureConfigId _configId;
    private readonly GetInfrastructureConfigQueryHandler _sut;

    public GetInfrastructureConfigQueryHandlerTests()
    {
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name(ConfigName), ProjectId.CreateUnique());
        _configId = _config.Id;
        _sut = new GetInfrastructureConfigQueryHandler(_accessService, _resourceGroupRepository, _mapper);
    }

    [Fact]
    public async Task Given_ReadAccessGranted_When_Handle_Then_ReturnsResultEnrichedWithCountsAsync()
    {
        // Arrange
        var initialDto = BuildBaseResult(_configId);
        _accessService.VerifyReadAccessAsync(_configId, Arg.Any<CancellationToken>())
            .Returns(_config);
        _mapper.Map<GetInfrastructureConfigResult>(_config).Returns(initialDto);
        _resourceGroupRepository.GetResourceCountsByInfraConfigIdsAsync(
                Arg.Any<IReadOnlyList<InfrastructureConfigId>>(),
                Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, (int ResourceGroupCount, int ResourceCount)>
            {
                [_configId.Value] = (ExpectedResourceGroupCount, ExpectedResourceCount),
            });
        var query = new GetInfrastructureConfigQuery(_configId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Id.Should().Be(_configId);
        result.Value.ResourceGroupCount.Should().Be(ExpectedResourceGroupCount);
        result.Value.ResourceCount.Should().Be(ExpectedResourceCount);
    }

    [Fact]
    public async Task Given_ReadAccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        _accessService.VerifyReadAccessAsync(_configId, Arg.Any<CancellationToken>())
            .Returns(Errors.InfrastructureConfig.NotFoundError(_configId));
        var query = new GetInfrastructureConfigQuery(_configId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _resourceGroupRepository.DidNotReceive().GetResourceCountsByInfraConfigIdsAsync(
            Arg.Any<IReadOnlyList<InfrastructureConfigId>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_NoCountsReturned_When_Handle_Then_KeepsZeroCountsAsync()
    {
        // Arrange
        var initialDto = BuildBaseResult(_configId);
        _accessService.VerifyReadAccessAsync(_configId, Arg.Any<CancellationToken>())
            .Returns(_config);
        _mapper.Map<GetInfrastructureConfigResult>(_config).Returns(initialDto);
        _resourceGroupRepository.GetResourceCountsByInfraConfigIdsAsync(
                Arg.Any<IReadOnlyList<InfrastructureConfigId>>(),
                Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, (int ResourceGroupCount, int ResourceCount)>());
        var query = new GetInfrastructureConfigQuery(_configId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ResourceGroupCount.Should().Be(0);
        result.Value.ResourceCount.Should().Be(0);
    }

    private static GetInfrastructureConfigResult BuildBaseResult(InfrastructureConfigId id) => new(
        id,
        new Name(ConfigName),
        ProjectId.CreateUnique(),
        DefaultNamingTemplate: null,
        UseProjectNamingConventions: true,
        ResourceNamingTemplates: [],
        ResourceAbbreviationOverrides: [],
        Tags: []);
}
