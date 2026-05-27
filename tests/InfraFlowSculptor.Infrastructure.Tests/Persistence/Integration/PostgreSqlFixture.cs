using InfraFlowSculptor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Integration;

/// <summary>
/// Shared xUnit fixture that starts a PostgreSQL container once per test collection.
/// Uses <see cref="IAsyncLifetime"/> for async container lifecycle management.
/// </summary>
public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private string _currentDb = "testdb_0";
    private int _dbCounter;

    /// <summary>Creates a new <see cref="ProjectDbContext"/> using the current database.</summary>
    public ProjectDbContext CreateContext()
    {
        var builder = new NpgsqlConnectionStringBuilder(_container.GetConnectionString())
        {
            Database = _currentDb
        };

        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql(builder.ConnectionString)
            .Options;

        return new ProjectDbContext(options);
    }

    /// <summary>Creates a fresh database for each test to ensure isolation.</summary>
    public async Task ResetDatabaseAsync()
    {
        _currentDb = $"testdb_{Interlocked.Increment(ref _dbCounter)}";

        // Create the new database via the default postgres connection
        var adminBuilder = new NpgsqlConnectionStringBuilder(_container.GetConnectionString())
        {
            Database = "postgres"
        };

        await using var conn = new NpgsqlConnection(adminBuilder.ConnectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"CREATE DATABASE \"{_currentDb}\"";
        await cmd.ExecuteNonQueryAsync();

        // Apply the EF model schema
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await ResetDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
