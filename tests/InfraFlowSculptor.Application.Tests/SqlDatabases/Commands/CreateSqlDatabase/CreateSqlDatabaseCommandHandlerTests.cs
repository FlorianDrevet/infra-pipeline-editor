using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.SqlDatabases.Commands.CreateSqlDatabase;
using InfraFlowSculptor.Application.SqlDatabases.Common;
using InfraFlowSculptor.Domain.Common.Errors;
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

namespace InfraFlowSculptor.Application.Tests.SqlDatabases.Commands.CreateSqlDatabase;

public sealed class CreateSqlDatabaseCommandHandlerTests
{
    private const string SqlDatabaseName = "sqldb-shared";
    private const string Collation = "SQL_Latin1_General_CP1_CI_AS";

    private readonly ISqlDatabaseRepository _sqlDatabaseRepository;
    private readonly ISqlServerRepository _sqlServerRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly SqlServer _sqlServer;
    private readonly CreateSqlDatabaseCommand _command;
    private readonly CreateSqlDatabaseCommandHandler _sut;

    public CreateSqlDatabaseCommandHandlerTests()
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
            new Name("sql-shared"),
            new Location(Location.LocationEnum.FranceCentral),
            new SqlServerVersion(SqlServerVersion.SqlServerVersionEnum.V12),
            "sqladmin");
        _command = new CreateSqlDatabaseCommand(
            _resourceGroup.Id,
            new Name(SqlDatabaseName),
            new Location(Location.LocationEnum.FranceCentral),
            SqlServerId: _sqlServer.Id.Value,
            Collation: Collation);
        _sqlDatabaseRepository.AddAsync(Arg.Any<SqlDatabase>())
            .Returns(callInfo => Task.FromResult((SqlDatabase)callInfo.Args()[0]));
        _sut = new CreateSqlDatabaseCommandHandler(
            _sqlDatabaseRepository, _sqlServerRepository, _resourceGroupRepository, _accessService, _mapper);
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
        await _sqlDatabaseRepository.DidNotReceive().AddAsync(Arg.Any<SqlDatabase>());
    }

    [Fact]
    public async Task Given_SqlServerNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
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
        await _sqlDatabaseRepository.DidNotReceive().AddAsync(Arg.Any<SqlDatabase>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsSqlDatabaseAndMapsResultAsync()
    {
        // Arrange
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
        await _sqlDatabaseRepository.Received(1).AddAsync(Arg.Is<SqlDatabase>(d =>
            d.ResourceGroupId == _resourceGroup.Id
            && d.Name.Value == SqlDatabaseName
            && d.SqlServerId == _sqlServer.Id
            && d.Collation == Collation));
        _mapper.Received(1).Map<SqlDatabaseResult>(Arg.Any<SqlDatabase>());
    }
}
