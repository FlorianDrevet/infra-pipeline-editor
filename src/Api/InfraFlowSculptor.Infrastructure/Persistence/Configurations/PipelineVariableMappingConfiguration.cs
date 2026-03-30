using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.PipelineVariableGroup;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="PipelineVariableMapping"/> entity.</summary>
public sealed class PipelineVariableMappingConfiguration
    : IEntityTypeConfiguration<PipelineVariableMapping>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PipelineVariableMapping> builder)
    {
        builder.ToTable("PipelineVariableMappings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<PipelineVariableMappingId>())
            .ValueGeneratedNever();

        builder.Property(x => x.VariableGroupId)
            .HasConversion(new IdValueConverter<PipelineVariableGroupId>())
            .IsRequired();

        builder.Property(x => x.PipelineVariableName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.BicepParameterName)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => new { x.VariableGroupId, x.BicepParameterName })
            .IsUnique();
    }
}
