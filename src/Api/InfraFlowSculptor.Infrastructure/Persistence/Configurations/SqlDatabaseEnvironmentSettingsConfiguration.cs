using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate.Entities;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="SqlDatabaseEnvironmentSettings"/> entity.</summary>
public class SqlDatabaseEnvironmentSettingsConfiguration
    : IEntityTypeConfiguration<SqlDatabaseEnvironmentSettings>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SqlDatabaseEnvironmentSettings> builder)
    {
        builder.ToTable("SqlDatabaseEnvironmentSettings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<SqlDatabaseEnvironmentSettingsId>());

        builder.Property(x => x.SqlDatabaseId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(x => x.EnvironmentName)
            .IsRequired();

        builder.Property(x => x.Sku)
            .HasConversion(
                v => (object?)v != null ? v.Value.ToString() : null,
                v => v != null
                    ? new SqlDatabaseSku(Enum.Parse<SqlDatabaseSku.SqlDatabaseSkuEnum>(v))
                    : null);

        builder.Property(x => x.MaxSizeGb);

        builder.Property(x => x.ZoneRedundant);
    }
}
