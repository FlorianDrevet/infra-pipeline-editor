using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

public sealed class CrossConfigResourceReferenceConfiguration
    : IEntityTypeConfiguration<CrossConfigResourceReference>
{
    public void Configure(EntityTypeBuilder<CrossConfigResourceReference> builder)
    {
        builder.ToTable("CrossConfigResourceReferences");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<CrossConfigResourceReferenceId>())
            .ValueGeneratedNever();

        builder.Property(x => x.InfraConfigId)
            .HasConversion(new IdValueConverter<InfrastructureConfigId>())
            .IsRequired();

        builder.Property(x => x.TargetConfigId)
            .HasConversion(new IdValueConverter<InfrastructureConfigId>())
            .IsRequired();

        builder.Property(x => x.TargetResourceId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.HasIndex(x => new { x.InfraConfigId, x.TargetResourceId })
            .IsUnique();
    }
}
