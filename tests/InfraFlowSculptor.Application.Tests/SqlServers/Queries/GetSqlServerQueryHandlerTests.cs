using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.SqlServers.Common;
using InfraFlowSculptor.Application.SqlServers.Queries;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.SqlServerAggregate;
using InfraFlowSculptor.Domain.SqlServerAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.SqlServers.Queries;

public sealed class GetSqlServerQueryHandlerTests
{
    private readonly ISqlServerRepository _sqlServerRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly SqlServer _sqlServer;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly GetSqlServerQuery _query;
    private readonly GetSqlServerQueryHandler _sut;

    public GetSqlServerQueryHandlerTests()
    {
        _sqlServerRepository = Substitute.For<ISqlServerRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _sqlServer = SqlServer.Create(
            _resourceGroup.Id,
            new Name("sql-shared"),
            new Location(Location.LocationEnum.FranceCentral),
            new SqlServerVersion(SqlServerVersion.SqlServerVersionEnum.V12),
            "sqladmin");
        _query = new GetSqlServerQuery(_sqlServer.Id);
        _sut = new GetSqlServerQueryHandler(
            _sqlServerRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_SqlServerNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _sqlServerRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((SqlServer?)null);

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
        _sqlServerRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_sqlServer);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _mapper.Received(1).Map<SqlServerResult>(_sqlServer);
    }
}
