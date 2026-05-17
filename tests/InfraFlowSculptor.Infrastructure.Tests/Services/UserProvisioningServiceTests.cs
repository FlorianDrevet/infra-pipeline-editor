using System.Collections;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence;
using InfraFlowSculptor.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Services;

public sealed class UserProvisioningServiceTests
{
    [Fact]
    public void Given_UserProvisioningAbstraction_When_InspectingInfrastructureAssembly_Then_ImplementationExists()
    {
        // Arrange
        var abstractionType = Type.GetType(
            "InfraFlowSculptor.Application.Common.Interfaces.Services.IUserProvisioningService, InfraFlowSculptor.Application");
        var implementationType = Type.GetType(
            "InfraFlowSculptor.Infrastructure.Services.UserProvisioningService, InfraFlowSculptor.Infrastructure");

        // Assert
        abstractionType.Should().NotBeNull();
        implementationType.Should().NotBeNull();
        abstractionType!.IsAssignableFrom(implementationType!).Should().BeTrue();
    }

    [Fact]
    public async Task Given_ClosedConnection_When_EnsureProvisionedAsync_Then_ExecutesProvisioningCommandAndClosesConnectionAsync()
    {
        // Arrange
        var provisionedUserId = Guid.NewGuid();
        var entraId = new EntraId(Guid.NewGuid());
        var name = new Name("Ada", "Lovelace");
        var connection = new RecordingDbConnection(provisionedUserId);
        await using var context = CreateRelationalContext(connection);
        var sut = new UserProvisioningService(context);

        // Act
        var result = await sut.EnsureProvisionedAsync(entraId, name);

        // Assert
        result.Value.Should().Be(provisionedUserId);
        connection.OpenAsyncCalls.Should().Be(1);
        connection.CloseAsyncCalls.Should().Be(1);
        connection.LastCommand.Should().NotBeNull();
        connection.LastCommand!.CommandText.Should().Contain("INSERT INTO \"User\"");
        var parameters = connection.LastCommand.RecordedParameters
            .ToDictionary(parameter => parameter.ParameterName);

        parameters.Should().ContainKey("id");
        parameters["id"].DbType.Should().Be(DbType.Guid);
        parameters["id"].Value.Should().BeOfType<Guid>();
        ((Guid)parameters["id"].Value!).Should().NotBe(Guid.Empty);
        parameters.Should().ContainKey("entraId");
        parameters["entraId"].DbType.Should().Be(DbType.Guid);
        parameters["entraId"].Value.Should().Be(entraId.Value);
        parameters.Should().ContainKey("firstName");
        parameters["firstName"].DbType.Should().Be(DbType.String);
        parameters["firstName"].Value.Should().Be(name.FirstName);
        parameters.Should().ContainKey("lastName");
        parameters["lastName"].DbType.Should().Be(DbType.String);
        parameters["lastName"].Value.Should().Be(name.LastName);
    }

    [Fact]
    public async Task Given_OpenConnection_When_EnsureProvisionedAsync_Then_ReusesConnectionStateAndSupportsStringResultsAsync()
    {
        // Arrange
        var provisionedUserId = Guid.NewGuid();
        var entraId = new EntraId(Guid.NewGuid());
        var name = new Name("Grace", "Hopper");
        var connection = new RecordingDbConnection(provisionedUserId.ToString(), ConnectionState.Open);
        await using var context = CreateRelationalContext(connection);
        var sut = new UserProvisioningService(context);

        // Act
        var result = await sut.EnsureProvisionedAsync(entraId, name);

        // Assert
        result.Value.Should().Be(provisionedUserId);
        connection.OpenAsyncCalls.Should().Be(0);
        connection.CloseAsyncCalls.Should().Be(0);
        connection.State.Should().Be(ConnectionState.Open);
    }

    private static ProjectDbContext CreateRelationalContext(DbConnection connection)
    {
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql(connection)
            .Options;

        return new ProjectDbContext(options);
    }
}

file sealed class RecordingDbConnection(object? executeScalarResult, ConnectionState initialState = ConnectionState.Closed) : DbConnection
{
    private ConnectionState state = initialState;

    public RecordingDbCommand? LastCommand { get; private set; }

    public int OpenAsyncCalls { get; private set; }

    public int CloseAsyncCalls { get; private set; }

    [AllowNull]
    public override string ConnectionString { get; set; } = "Host=localhost;Database=user_provisioning_tests;Username=test;Password=test";

    public override string Database => "user_provisioning_tests";

    public override string DataSource => "localhost";

    public override string ServerVersion => "16.0";

    public override ConnectionState State => state;

    public override void ChangeDatabase(string databaseName)
    {
    }

    public override void Close()
    {
        state = ConnectionState.Closed;
    }

    public override Task CloseAsync()
    {
        CloseAsyncCalls++;
        state = ConnectionState.Closed;
        return Task.CompletedTask;
    }

    public override void Open()
    {
        state = ConnectionState.Open;
    }

    public override Task OpenAsync(CancellationToken cancellationToken)
    {
        OpenAsyncCalls++;
        state = ConnectionState.Open;
        return Task.CompletedTask;
    }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        throw new NotSupportedException();
    }

    protected override DbCommand CreateDbCommand()
    {
        LastCommand = new RecordingDbCommand(this, executeScalarResult);
        return LastCommand;
    }
}

file sealed class RecordingDbCommand(DbConnection connection, object? executeScalarResult) : DbCommand
{
    private readonly RecordingDbParameterCollection parameters = [];

    public IReadOnlyList<DbParameter> RecordedParameters => parameters.Items;

    [AllowNull]
    public override string CommandText { get; set; } = string.Empty;

    public override int CommandTimeout { get; set; }

    public override CommandType CommandType { get; set; }

    public override bool DesignTimeVisible { get; set; }

    public override UpdateRowSource UpdatedRowSource { get; set; }

    protected override DbConnection? DbConnection { get; set; } = connection;

    protected override DbParameterCollection DbParameterCollection => parameters;

    protected override DbTransaction? DbTransaction { get; set; }

    public override void Cancel()
    {
    }

    public override int ExecuteNonQuery()
    {
        throw new NotSupportedException();
    }

    public override object? ExecuteScalar()
    {
        return executeScalarResult;
    }

    public override Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(executeScalarResult);
    }

    public override void Prepare()
    {
    }

    protected override DbParameter CreateDbParameter()
    {
        return new RecordingDbParameter();
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        throw new NotSupportedException();
    }
}

file sealed class RecordingDbParameter : DbParameter
{
    public override DbType DbType { get; set; }

    public override ParameterDirection Direction { get; set; } = ParameterDirection.Input;

    public override bool IsNullable { get; set; }

    [AllowNull]
    public override string ParameterName { get; set; } = string.Empty;

    [AllowNull]
    public override string SourceColumn { get; set; } = string.Empty;

    public override object? Value { get; set; }

    public override bool SourceColumnNullMapping { get; set; }

    public override int Size { get; set; }

    public override void ResetDbType()
    {
    }
}

file sealed class RecordingDbParameterCollection : DbParameterCollection
{
    private readonly List<DbParameter> items = [];

    public IReadOnlyList<DbParameter> Items => items;

    public override int Count => items.Count;

    public override object SyncRoot => ((ICollection)items).SyncRoot!;

    public override int Add(object value)
    {
        items.Add((DbParameter)value);
        return items.Count - 1;
    }

    public override void AddRange(Array values)
    {
        foreach (var value in values)
        {
            Add(value!);
        }
    }

    public override void Clear()
    {
        items.Clear();
    }

    public override bool Contains(object value)
    {
        return items.Contains((DbParameter)value);
    }

    public override bool Contains(string value)
    {
        return items.Any(parameter => parameter.ParameterName == value);
    }

    public override void CopyTo(Array array, int index)
    {
        items.ToArray().CopyTo(array, index);
    }

    public override IEnumerator GetEnumerator()
    {
        return items.GetEnumerator();
    }

    public override int IndexOf(object value)
    {
        return items.IndexOf((DbParameter)value);
    }

    public override int IndexOf(string parameterName)
    {
        return items.FindIndex(parameter => parameter.ParameterName == parameterName);
    }

    public override void Insert(int index, object value)
    {
        items.Insert(index, (DbParameter)value);
    }

    public override void Remove(object value)
    {
        items.Remove((DbParameter)value);
    }

    public override void RemoveAt(int index)
    {
        items.RemoveAt(index);
    }

    public override void RemoveAt(string parameterName)
    {
        var index = IndexOf(parameterName);
        if (index >= 0)
        {
            RemoveAt(index);
        }
    }

    protected override DbParameter GetParameter(int index)
    {
        return items[index];
    }

    protected override DbParameter GetParameter(string parameterName)
    {
        return items[IndexOf(parameterName)];
    }

    protected override void SetParameter(int index, DbParameter value)
    {
        items[index] = value;
    }

    protected override void SetParameter(string parameterName, DbParameter value)
    {
        var index = IndexOf(parameterName);
        if (index >= 0)
        {
            items[index] = value;
            return;
        }

        items.Add(value);
    }
}
