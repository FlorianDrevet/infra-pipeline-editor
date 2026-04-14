using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate.Entities;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="EventHubNamespaceEnvironmentSettings"/> entity.</summary>
public sealed class EventHubNamespaceEnvironmentSettingsConfiguration
    : IEntityTypeConfiguration<EventHubNamespaceEnvironmentSettings>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<EventHubNamespaceEnvironmentSettings> builder)
    {
        builder.ToTable("EventHubNamespaceEnvironmentSettings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<EventHubNamespaceEnvironmentSettingsId>());

        builder.Property(x => x.EventHubNamespaceId)
            .IsRequired()
            .HasConversion(new IdValueConverter<AzureResourceId>());

        builder.Property(x => x.EnvironmentName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Sku)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(x => x.Capacity)
            .IsRequired(false);

        builder.Property(x => x.ZoneRedundant)
            .IsRequired(false);

        builder.Property(x => x.DisableLocalAuth)
            .IsRequired(false);

        builder.Property(x => x.MinimumTlsVersion)
            .IsRequired(false)
            .HasMaxLength(10);

        builder.Property(x => x.AutoInflateEnabled)
            .IsRequired(false);

        builder.Property(x => x.MaxThroughputUnits)
            .IsRequired(false);

        builder.HasIndex(x => new { x.EventHubNamespaceId, x.EnvironmentName })
            .IsUnique();
    }
}
