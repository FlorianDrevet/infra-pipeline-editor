using System.Collections;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using InfraFlowSculptor.Infrastructure.Extensions;
using InfraFlowSculptor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Extensions;

public sealed class ProjectDbContextBaselineHistorySynchronizerTests
{
    [Fact]
    public async Task Given_ExistingProjectSchemaWithoutBaselineMigration_When_SynchronizeAsync_Then_CreatesHistoryTableAndInsertsInitialCreateAsync()
    {
        // Arrange
        var connection = new BaselineRecordingDbConnection(existingMarkerTableCount: 4, appliedBaselineMigrationCount: 0);
        await using var context = CreateRelationalContext(connection);

        // Act
        var wasSynchronized = await ProjectDbContextBaselineHistorySynchronizer.SynchronizeAsync(context);

        // Assert
        wasSynchronized.Should().BeTrue();
        connection.OpenAsyncCalls.Should().Be(1);
        connection.CloseAsyncCalls.Should().Be(1);
        connection.ExecutedCommands.Should().Contain(command =>
            command.CommandText.Contains("information_schema.tables", StringComparison.Ordinal));
        connection.ExecutedCommands.Should().Contain(command =>
            command.CommandText.Contains("CREATE TABLE IF NOT EXISTS \"__EFMigrationsHistory\"", StringComparison.Ordinal));
        connection.ExecutedCommands.Should().Contain(command =>
            command.CommandText.Contains("INSERT INTO \"__EFMigrationsHistory\"", StringComparison.Ordinal));

        var insertCommand = connection.ExecutedCommands.Single(command =>
            command.CommandText.Contains("INSERT INTO \"__EFMigrationsHistory\"", StringComparison.Ordinal));

        var migrationIdParameter = insertCommand.Parameters.Single(parameter => parameter.ParameterName == "migrationId");
        migrationIdParameter.Value.Should().Be("20260517074017_InitialCreate");

        var productVersionParameter = insertCommand.Parameters.Single(parameter => parameter.ParameterName == "productVersion");
        productVersionParameter.Value.Should().BeOfType<string>();
        ((string)productVersionParameter.Value!).Should().NotBeEmpty();
    }

    [Fact]
    public async Task Given_BaselinedLegacyProjectSchemaMissingAcrPullIdentityColumns_When_SynchronizeAsync_Then_RepairsComputeTablesAsync()
    {
        // Arrange
        var connection = new BaselineRecordingDbConnection(existingMarkerTableCount: 4, appliedBaselineMigrationCount: 1);
        await using var context = CreateRelationalContext(connection);

        // Act
        var wasSynchronized = await ProjectDbContextBaselineHistorySynchronizer.SynchronizeAsync(context);

        // Assert
        wasSynchronized.Should().BeFalse();
        connection.ExecutedCommands.Should().Contain(command =>
            command.CommandText.Contains("ALTER TABLE IF EXISTS \"ContainerApps\" ADD COLUMN IF NOT EXISTS \"AcrPullIdentityId\"", StringComparison.Ordinal));
        connection.ExecutedCommands.Should().Contain(command =>
            command.CommandText.Contains("ALTER TABLE IF EXISTS \"FunctionApps\" ADD COLUMN IF NOT EXISTS \"AcrPullIdentityId\"", StringComparison.Ordinal));
        connection.ExecutedCommands.Should().Contain(command =>
            command.CommandText.Contains("ALTER TABLE IF EXISTS \"WebApps\" ADD COLUMN IF NOT EXISTS \"AcrPullIdentityId\"", StringComparison.Ordinal));
        connection.ExecutedCommands.Should().Contain(command =>
            command.CommandText.Contains("UPDATE \"ContainerApps\" AS computeResource", StringComparison.Ordinal)
            && command.CommandText.Contains("\"AssignedUserAssignedIdentityId\"", StringComparison.Ordinal));
        connection.ExecutedCommands.Should().Contain(command =>
            command.CommandText.Contains("UPDATE \"FunctionApps\" AS computeResource", StringComparison.Ordinal)
            && command.CommandText.Contains("\"AssignedUserAssignedIdentityId\"", StringComparison.Ordinal));
        connection.ExecutedCommands.Should().Contain(command =>
            command.CommandText.Contains("UPDATE \"WebApps\" AS computeResource", StringComparison.Ordinal)
            && command.CommandText.Contains("\"AssignedUserAssignedIdentityId\"", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Given_EmptyDatabase_When_SynchronizeAsync_Then_DoesNotTouchMigrationHistoryAsync()
    {
        // Arrange
        var connection = new BaselineRecordingDbConnection(existingMarkerTableCount: 0, appliedBaselineMigrationCount: 0);
        await using var context = CreateRelationalContext(connection);

        // Act
        var wasSynchronized = await ProjectDbContextBaselineHistorySynchronizer.SynchronizeAsync(context);

        // Assert
        wasSynchronized.Should().BeFalse();
        connection.ExecutedCommands.Should().ContainSingle(command =>
            command.CommandText.Contains("information_schema.tables", StringComparison.Ordinal));
        connection.ExecutedCommands.Should().NotContain(command =>
            command.CommandText.Contains("__EFMigrationsHistory", StringComparison.Ordinal));
    }

    private static ProjectDbContext CreateRelationalContext(DbConnection connection)
    {
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql(connection)
            .Options;

        return new ProjectDbContext(options);
    }
}

file sealed class BaselineRecordingDbConnection(int existingMarkerTableCount, int appliedBaselineMigrationCount, ConnectionState initialState = ConnectionState.Closed)
    : DbConnection
{
    private ConnectionState state = initialState;

    public List<RecordedDbCommand> ExecutedCommands { get; } = [];

    public int OpenAsyncCalls { get; private set; }

    public int CloseAsyncCalls { get; private set; }

    [AllowNull]
    public override string ConnectionString { get; set; } = "Host=localhost;Database=baseline_sync_tests;Username=test;Password=test";

    public override string Database => "baseline_sync_tests";

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
        return new BaselineRecordingDbCommand(this, existingMarkerTableCount, appliedBaselineMigrationCount);
    }

    public void RecordCommand(string commandText, IReadOnlyList<DbParameter> parameters)
    {
        ExecutedCommands.Add(new RecordedDbCommand(commandText, parameters
            .Select(parameter => new RecordedDbParameter(parameter.ParameterName, parameter.Value))
            .ToList()));
    }
}

file sealed class BaselineRecordingDbCommand(
    BaselineRecordingDbConnection connection,
    int existingMarkerTableCount,
    int appliedBaselineMigrationCount)
    : DbCommand
{
    private readonly RecordingDbParameterCollection parameters = [];

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
        connection.RecordCommand(CommandText, parameters.Items);
        return 1;
    }

    public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
    {
        connection.RecordCommand(CommandText, parameters.Items);
        return Task.FromResult(1);
    }

    public override object? ExecuteScalar()
    {
        connection.RecordCommand(CommandText, parameters.Items);
        return GetScalarResult();
    }

    public override Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken)
    {
        connection.RecordCommand(CommandText, parameters.Items);
        return Task.FromResult<object?>(GetScalarResult());
    }

    public override void Prepare()
    {
    }

    protected override DbParameter CreateDbParameter()
    {
        return new MutableDbParameter();
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        throw new NotSupportedException();
    }

    private object GetScalarResult()
    {
        if (CommandText.Contains("information_schema.tables", StringComparison.Ordinal))
        {
            return existingMarkerTableCount;
        }

        if (CommandText.Contains("FROM \"__EFMigrationsHistory\"", StringComparison.Ordinal))
        {
            return appliedBaselineMigrationCount;
        }

        throw new InvalidOperationException($"No scalar result configured for command: {CommandText}");
    }
}

file sealed class MutableDbParameter : DbParameter
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

file sealed record RecordedDbCommand(string CommandText, IReadOnlyList<RecordedDbParameter> Parameters);

file sealed record RecordedDbParameter(string ParameterName, object? Value);