using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.SqlServers.Commands.CreateSqlServer;
using InfraFlowSculptor.Application.SqlServers.Common;
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

namespace InfraFlowSculptor.Application.Tests.SqlServers.Commands.CreateSqlServer;

public sealed class CreateSqlServerCommandHandlerTests
{
    private const string SqlServerName = "sql-shared";
    private const string AdministratorLogin = "sqladmin";

    private readonly ISqlServerRepository _sqlServerRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly CreateSqlServerCommand _command;
    private readonly CreateSqlServerCommandHandler _sut;

    public CreateSqlServerCommandHandlerTests()
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
        _command = new CreateSqlServerCommand(
            _resourceGroup.Id,
            new Name(SqlServerName),
            new Location(Location.LocationEnum.FranceCentral),
            Version: nameof(SqlServerVersion.SqlServerVersionEnum.V12),
            AdministratorLogin: AdministratorLogin);
        _sqlServerRepository.AddAsync(Arg.Any<SqlServer>())
            .Returns(callInfo => Task.FromResult((SqlServer)callInfo.Args()[0]));
        _sut = new CreateSqlServerCommandHandler(
            _sqlServerRepository, _resourceGroupRepository, _accessService, _mapper);
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
        await _sqlServerRepository.DidNotReceive().AddAsync(Arg.Any<SqlServer>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsSqlServerAndMapsResultAsync()
    {
        // Arrange
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        await _sqlServerRepository.Received(1).AddAsync(Arg.Is<SqlServer>(s =>
            s.ResourceGroupId == _resourceGroup.Id
            && s.Name.Value == SqlServerName
            && s.AdministratorLogin == AdministratorLogin));
        _mapper.Received(1).Map<SqlServerResult>(Arg.Any<SqlServer>());
    }
}
