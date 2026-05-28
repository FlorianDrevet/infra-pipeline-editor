using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.DocumentIntelligenceAggregate.Entities;
using InfraFlowSculptor.Domain.DocumentIntelligenceAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="DocumentIntelligenceEnvironmentSettings"/> entity.
/// </summary>
public sealed class DocumentIntelligenceEnvironmentSettingsConfiguration
    : IEntityTypeConfiguration<DocumentIntelligenceEnvironmentSettings>
{
    public void Configure(EntityTypeBuilder<DocumentIntelligenceEnvironmentSettings> builder)
    {
        builder.ToTable("DocumentIntelligenceEnvironmentSettings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<DocumentIntelligenceEnvironmentSettingsId>());

        builder.Property(x => x.DocumentIntelligenceId)
            .IsRequired()
            .HasConversion(new IdValueConverter<AzureResourceId>());

        builder.Property(x => x.EnvironmentName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Sku)
            .IsRequired(false)
#pragma warning disable CS8620 // Nullability mismatch — EF Core handles null conversion internally
            .HasConversion(new EnumValueConverter<DocumentIntelligenceSku, DocumentIntelligenceSku.Sku>());
#pragma warning restore CS8620

        builder.Property(x => x.PublicNetworkAccess)
            .IsRequired(false)
#pragma warning disable CS8620 // Nullability mismatch — EF Core handles null conversion internally
            .HasConversion(new EnumValueConverter<PublicNetworkAccessMode, PublicNetworkAccessMode.Mode>());
#pragma warning restore CS8620

        builder.Property(x => x.DisableLocalAuth)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(x => new { x.DocumentIntelligenceId, x.EnvironmentName })
            .IsUnique();
    }
}
