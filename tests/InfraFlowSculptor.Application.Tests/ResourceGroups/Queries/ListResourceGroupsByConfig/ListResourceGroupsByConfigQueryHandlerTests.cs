using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Application.ResourceGroups.Queries.ListResourceGroupsByConfig;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.ResourceGroups.Queries.ListResourceGroupsByConfig;

public sealed class ListResourceGroupsByConfigQueryHandlerTests
{
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly ListResourceGroupsByConfigQueryHandler _sut;

    public ListResourceGroupsByConfigQueryHandlerTests()
    {
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _sut = new ListResourceGroupsByConfigQueryHandler(_resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsNotFoundErrorAsync()
    {
        // Arrange
        var configId = InfrastructureConfigId.CreateUnique();
        var query = new ListResourceGroupsByConfigQuery(configId);
        _accessService.VerifyReadAccessAsync(configId, Arg.Any<CancellationToken>())
            .Returns(Error.NotFound("InfrastructureConfig.NotFound", "Not found"));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_ValidAccess_When_Handle_Then_ReturnsMappedResourceGroupsAsync()
    {
        // Arrange
        var projectId = ProjectId.CreateUnique();
        var config = DomainInfrastructureConfig.Create(new Name("Config"), projectId);
        var configId = config.Id;
        var query = new ListResourceGroupsByConfigQuery(configId);

        _accessService.VerifyReadAccessAsync(configId, Arg.Any<CancellationToken>())
            .Returns(config);

        var rg = DomainResourceGroup.Create(new Name("rg-main"), configId,
            new Domain.Common.ValueObjects.Location(Domain.Common.ValueObjects.Location.LocationEnum.WestEurope));
        var rgResult = new ResourceGroupResult(rg.Id, configId, rg.Location, rg.Name, []);

        _resourceGroupRepository.GetLightweightByInfraConfigIdAsync(configId, Arg.Any<CancellationToken>())
            .Returns(new List<DomainResourceGroup> { rg });
        _mapper.Map<ResourceGroupResult>(rg).Returns(rgResult);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
        result.Value[0].Name.Value.Should().Be("rg-main");
    }

    [Fact]
    public async Task Given_NoResourceGroups_When_Handle_Then_ReturnsEmptyListAsync()
    {
        // Arrange
        var projectId = ProjectId.CreateUnique();
        var config = DomainInfrastructureConfig.Create(new Name("Config"), projectId);
        var configId = config.Id;
        var query = new ListResourceGroupsByConfigQuery(configId);

        _accessService.VerifyReadAccessAsync(configId, Arg.Any<CancellationToken>())
            .Returns(config);
        _resourceGroupRepository.GetLightweightByInfraConfigIdAsync(configId, Arg.Any<CancellationToken>())
            .Returns(new List<DomainResourceGroup>());

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }
}
