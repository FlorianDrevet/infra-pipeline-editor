using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="SqlDatabase"/> aggregate.</summary>
public class SqlDatabaseConfiguration : IEntityTypeConfiguration<SqlDatabase>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SqlDatabase> builder)
    {
        ConfigureTable(builder);
    }

    private static void ConfigureTable(EntityTypeBuilder<SqlDatabase> builder)
    {
        builder.HasBaseType<AzureResource>()
            .ToTable("SqlDatabases");

        builder.Property(x => x.SqlServerId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(x => x.Collation)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasMany(x => x.EnvironmentSettings)
            .WithOne()
            .HasForeignKey(es => es.SqlDatabaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.EnvironmentSettings)
            .HasField("_environmentSettings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
