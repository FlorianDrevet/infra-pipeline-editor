using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.Entites;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="SecureParameterMapping"/> entity.</summary>
public sealed class SecureParameterMappingConfiguration : IEntityTypeConfiguration<SecureParameterMapping>
{
    private const string TableName = "SecureParameterMappings";

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SecureParameterMapping> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasConversion(new IdValueConverter<SecureParameterMappingId>())
            .ValueGeneratedNever();

        builder.Property(m => m.ResourceId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(m => m.SecureParameterName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.VariableGroupId)
            .HasConversion(
                v => v == null ? (Guid?)null : v.Value,
                v => v.HasValue ? new ProjectPipelineVariableGroupId(v.Value) : null)
            .IsRequired(false);

        builder.Property(m => m.PipelineVariableName)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.HasIndex(m => new { m.ResourceId, m.SecureParameterName })
            .IsUnique();

        // Note: the AzureResource → SecureParameterMappings relationship (HasMany/WithOne)
        // is configured in AzureResourceConfiguration to keep orphan deletion working correctly.

        builder.HasOne<ProjectPipelineVariableGroup>()
            .WithMany()
            .HasForeignKey(m => m.VariableGroupId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
