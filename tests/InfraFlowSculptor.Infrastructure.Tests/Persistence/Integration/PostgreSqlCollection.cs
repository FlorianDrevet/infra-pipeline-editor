using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Integration;

/// <summary>Shares the <see cref="PostgreSqlFixture"/> across all integration tests.</summary>
[CollectionDefinition(Name)]
public sealed class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>
{
    public const string Name = "PostgreSQL";
}
