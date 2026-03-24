using InfraFlowSculptor.Domain.ApplicationInsightsAggregate.Entities;
using InfraFlowSculptor.Domain.ApplicationInsightsAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="ApplicationInsightsEnvironmentSettings"/> entity.
/// </summary>
public sealed class ApplicationInsightsEnvironmentSettingsConfiguration
    : IEntityTypeConfiguration<ApplicationInsightsEnvironmentSettings>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ApplicationInsightsEnvironmentSettings> builder)
    {
        builder.ToTable("ApplicationInsightsEnvironmentSettings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<ApplicationInsightsEnvironmentSettingsId>());

        builder.Property(x => x.ApplicationInsightsId)
            .IsRequired()
            .HasConversion(new IdValueConverter<AzureResourceId>());

        builder.Property(x => x.EnvironmentName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.SamplingPercentage)
            .IsRequired(false);

        builder.Property(x => x.RetentionInDays)
            .IsRequired(false);

        builder.Property(x => x.DisableIpMasking)
            .IsRequired(false);

        builder.Property(x => x.DisableLocalAuth)
            .IsRequired(false);

        builder.Property(x => x.IngestionMode)
            .IsRequired(false)
            .HasMaxLength(100);

        builder.HasIndex(x => new { x.ApplicationInsightsId, x.EnvironmentName })
            .IsUnique();
    }
}
