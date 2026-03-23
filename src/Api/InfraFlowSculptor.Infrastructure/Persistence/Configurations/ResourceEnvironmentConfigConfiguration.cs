using System.Text.Json;
using InfraFlowSculptor.Domain.Common.BaseModels.Entites;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="ResourceEnvironmentConfig"/> entity.
/// Stores per-environment configuration properties as a JSON column.
/// </summary>
public sealed class ResourceEnvironmentConfigConfiguration
    : IEntityTypeConfiguration<ResourceEnvironmentConfig>
{
    public void Configure(EntityTypeBuilder<ResourceEnvironmentConfig> builder)
    {
        builder.ToTable("ResourceEnvironmentConfigs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<ResourceEnvironmentConfigId>());

        builder.Property(x => x.ResourceId)
            .IsRequired()
            .HasConversion(new IdValueConverter<AzureResourceId>());

        builder.Property(x => x.EnvironmentName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Properties)
            .IsRequired()
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null)
                     ?? new Dictionary<string, string>());

        builder.HasIndex(x => new { x.ResourceId, x.EnvironmentName })
            .IsUnique();
    }
}
