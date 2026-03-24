using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.SqlServerAggregate.Entities;
using InfraFlowSculptor.Domain.SqlServerAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="SqlServerEnvironmentSettings"/> entity.</summary>
public class SqlServerEnvironmentSettingsConfiguration
    : IEntityTypeConfiguration<SqlServerEnvironmentSettings>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SqlServerEnvironmentSettings> builder)
    {
        builder.ToTable("SqlServerEnvironmentSettings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<SqlServerEnvironmentSettingsId>());

        builder.Property(x => x.SqlServerId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(x => x.EnvironmentName)
            .IsRequired();

        builder.Property(x => x.MinimalTlsVersion)
            .HasMaxLength(10);
    }
}
