using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.PipelineVariableGroup;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="PipelineVariableGroup"/> entity.</summary>
public sealed class PipelineVariableGroupConfiguration
    : IEntityTypeConfiguration<PipelineVariableGroup>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PipelineVariableGroup> builder)
    {
        builder.ToTable("PipelineVariableGroups");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<PipelineVariableGroupId>())
            .ValueGeneratedNever();

        builder.Property(x => x.InfraConfigId)
            .HasConversion(new IdValueConverter<InfrastructureConfigId>())
            .IsRequired();

        builder.Property(x => x.GroupName)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => new { x.InfraConfigId, x.GroupName })
            .IsUnique();

        builder.HasMany(x => x.Mappings)
            .WithOne()
            .HasForeignKey(x => x.VariableGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
