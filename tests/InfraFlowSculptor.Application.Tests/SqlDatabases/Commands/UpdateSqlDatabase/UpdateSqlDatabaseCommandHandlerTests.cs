using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.SqlDatabases.Commands.UpdateSqlDatabase;
using InfraFlowSculptor.Application.SqlDatabases.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate;
using InfraFlowSculptor.Domain.SqlServerAggregate;
using InfraFlowSculptor.Domain.SqlServerAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.SqlDatabases.Commands.UpdateSqlDatabase;

public sealed class UpdateSqlDatabaseCommandHandlerTests
{
    private readonly ISqlDatabaseRepository _sqlDatabaseRepository;
    private readonly ISqlServerRepository _sqlServerRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly SqlServer _sqlServer;
    private readonly SqlDatabase _existingEntity;
    private readonly UpdateSqlDatabaseCommand _command;
    private readonly UpdateSqlDatabaseCommandHandler _sut;

    public UpdateSqlDatabaseCommandHandlerTests()
    {
        _sqlDatabaseRepository = Substitute.For<ISqlDatabaseRepository>();
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
            new Name("sql-srv"),
            new Location(Location.LocationEnum.FranceCentral),
            new SqlServerVersion(SqlServerVersion.SqlServerVersionEnum.V12),
            "sqladmin");
        _existingEntity = SqlDatabase.Create(
            _resourceGroup.Id,
            new Name("sqldb-old"),
            new Location(Location.LocationEnum.FranceCentral),
            _sqlServer.Id,
            "SQL_Latin1_General_CP1_CI_AS");
        _command = new UpdateSqlDatabaseCommand(
            _existingEntity.Id,
            new Name("sqldb-renamed"),
            new Location(Location.LocationEnum.WestEurope),
            SqlServerId: _sqlServer.Id.Value,
            Collation: "SQL_Latin1_General_CP1_CI_AS");
        _sqlDatabaseRepository.Update(Arg.Any<SqlDatabase>())
            .Returns(callInfo => (SqlDatabase)callInfo.Args()[0]);
        _sut = new UpdateSqlDatabaseCommandHandler(
            _sqlDatabaseRepository, _sqlServerRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_EntityNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _sqlDatabaseRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((SqlDatabase?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _sqlDatabaseRepository.DidNotReceive().Update(Arg.Any<SqlDatabase>());
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _sqlDatabaseRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _sqlDatabaseRepository.DidNotReceive().Update(Arg.Any<SqlDatabase>());
    }

    [Fact]
    public async Task Given_SqlServerNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _sqlDatabaseRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _sqlServerRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((SqlServer?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _sqlDatabaseRepository.DidNotReceive().Update(Arg.Any<SqlDatabase>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsUpdatedEntityAndMapsResultAsync()
    {
        // Arrange
        _sqlDatabaseRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _sqlServerRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_sqlServer);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _sqlDatabaseRepository.Received(1).Update(Arg.Is<SqlDatabase>(d =>
            d.Name.Value == "sqldb-renamed"));
        _mapper.Received(1).Map<SqlDatabaseResult>(Arg.Any<SqlDatabase>());
    }
}
