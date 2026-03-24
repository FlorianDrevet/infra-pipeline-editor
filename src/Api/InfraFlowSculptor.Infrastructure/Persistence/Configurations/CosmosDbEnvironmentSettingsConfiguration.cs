using InfraFlowSculptor.Domain.CosmosDbAggregate.Entities;
using InfraFlowSculptor.Domain.CosmosDbAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="CosmosDbEnvironmentSettings"/> entity.
/// </summary>
public sealed class CosmosDbEnvironmentSettingsConfiguration
    : IEntityTypeConfiguration<CosmosDbEnvironmentSettings>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CosmosDbEnvironmentSettings> builder)
    {
        builder.ToTable("CosmosDbEnvironmentSettings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<CosmosDbEnvironmentSettingsId>());

        builder.Property(x => x.CosmosDbId)
            .IsRequired()
            .HasConversion(new IdValueConverter<AzureResourceId>());

        builder.Property(x => x.EnvironmentName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.DatabaseApiType)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(x => x.ConsistencyLevel)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(x => x.MaxStalenessPrefix)
            .IsRequired(false);

        builder.Property(x => x.MaxIntervalInSeconds)
            .IsRequired(false);

        builder.Property(x => x.EnableAutomaticFailover)
            .IsRequired(false);

        builder.Property(x => x.EnableMultipleWriteLocations)
            .IsRequired(false);

        builder.Property(x => x.BackupPolicyType)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(x => x.EnableFreeTier)
            .IsRequired(false);

        builder.HasIndex(x => new { x.CosmosDbId, x.EnvironmentName })
            .IsUnique();
    }
}
