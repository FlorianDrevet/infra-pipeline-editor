using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.SqlDatabases.Commands.DeleteSqlDatabase;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.SqlDatabases.Commands.DeleteSqlDatabase;

public sealed class DeleteSqlDatabaseCommandHandlerTests
{
    private readonly ISqlDatabaseRepository _sqlDatabaseRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly SqlDatabase _sqlDatabase;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly DeleteSqlDatabaseCommand _command;
    private readonly DeleteSqlDatabaseCommandHandler _sut;

    public DeleteSqlDatabaseCommandHandlerTests()
    {
        _sqlDatabaseRepository = Substitute.For<ISqlDatabaseRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _sqlDatabase = SqlDatabase.Create(
            _resourceGroup.Id,
            new Name("sqldb-shared"),
            new Location(Location.LocationEnum.FranceCentral),
            AzureResourceId.CreateUnique(),
            "SQL_Latin1_General_CP1_CI_AS");
        _command = new DeleteSqlDatabaseCommand(_sqlDatabase.Id);
        _sut = new DeleteSqlDatabaseCommandHandler(
            _sqlDatabaseRepository, _resourceGroupRepository, _accessService);
    }

    [Fact]
    public async Task Given_SqlDatabaseNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _sqlDatabaseRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((SqlDatabase?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _sqlDatabaseRepository.DidNotReceive().DeleteAsync(Arg.Any<AzureResourceId>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_DeletesSqlDatabaseAsync()
    {
        // Arrange
        _sqlDatabaseRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_sqlDatabase);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        await _sqlDatabaseRepository.Received(1).DeleteAsync(_sqlDatabase.Id);
    }
}
