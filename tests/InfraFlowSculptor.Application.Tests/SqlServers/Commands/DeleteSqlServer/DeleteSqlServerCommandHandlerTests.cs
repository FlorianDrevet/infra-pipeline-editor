using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.SqlServers.Commands.DeleteSqlServer;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate;
using InfraFlowSculptor.Domain.SqlServerAggregate;
using InfraFlowSculptor.Domain.SqlServerAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.SqlServers.Commands.DeleteSqlServer;

public sealed class DeleteSqlServerCommandHandlerTests
{
    private readonly ISqlServerRepository _sqlServerRepository;
    private readonly ISqlDatabaseRepository _sqlDatabaseRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly SqlServer _sqlServer;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly DeleteSqlServerCommand _command;
    private readonly DeleteSqlServerCommandHandler _sut;

    public DeleteSqlServerCommandHandlerTests()
    {
        _sqlServerRepository = Substitute.For<ISqlServerRepository>();
        _sqlDatabaseRepository = Substitute.For<ISqlDatabaseRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
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
        _command = new DeleteSqlServerCommand(_sqlServer.Id);
        _sqlDatabaseRepository.GetBySqlServerIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(new List<SqlDatabase>());
        _sut = new DeleteSqlServerCommandHandler(
            _sqlServerRepository, _sqlDatabaseRepository, _resourceGroupRepository, _accessService);
    }

    [Fact]
    public async Task Given_SqlServerNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _sqlServerRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((SqlServer?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _sqlServerRepository.DidNotReceive().DeleteAsync(Arg.Any<AzureResourceId>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_DeletesSqlServerAsync()
    {
        // Arrange
        _sqlServerRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_sqlServer);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        await _sqlServerRepository.Received(1).DeleteAsync(_sqlServer.Id);
    }
}
