using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for <see cref="ResourceAbbreviationOverride"/>.</summary>
public sealed class ResourceAbbreviationOverrideConfiguration
    : IEntityTypeConfiguration<ResourceAbbreviationOverride>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ResourceAbbreviationOverride> builder)
    {
        builder.ToTable("ResourceAbbreviationOverrides");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<ResourceAbbreviationOverrideId>())
            .ValueGeneratedNever();

        builder.Property(x => x.InfraConfigId)
            .HasConversion(new IdValueConverter<InfrastructureConfigId>());

        builder.Property(x => x.ResourceType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Abbreviation)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(x => new { x.InfraConfigId, x.ResourceType })
            .IsUnique();
    }
}
