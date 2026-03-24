using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.LogAnalyticsWorkspaceAggregate.Entities;
using InfraFlowSculptor.Domain.LogAnalyticsWorkspaceAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="LogAnalyticsWorkspaceEnvironmentSettings"/> entity.
/// </summary>
public sealed class LogAnalyticsWorkspaceEnvironmentSettingsConfiguration
    : IEntityTypeConfiguration<LogAnalyticsWorkspaceEnvironmentSettings>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LogAnalyticsWorkspaceEnvironmentSettings> builder)
    {
        builder.ToTable("LogAnalyticsWorkspaceEnvironmentSettings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<LogAnalyticsWorkspaceEnvironmentSettingsId>());

        builder.Property(x => x.LogAnalyticsWorkspaceId)
            .IsRequired()
            .HasConversion(new IdValueConverter<AzureResourceId>());

        builder.Property(x => x.EnvironmentName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Sku)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(x => x.RetentionInDays)
            .IsRequired(false);

        builder.Property(x => x.DailyQuotaGb)
            .IsRequired(false);

        builder.HasIndex(x => new { x.LogAnalyticsWorkspaceId, x.EnvironmentName })
            .IsUnique();
    }
}
