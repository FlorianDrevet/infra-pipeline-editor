using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ParameterDefinition;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

public sealed class ResourceParameterUsageConfiguration
    : IEntityTypeConfiguration<ResourceParameterUsage>
{
    public void Configure(EntityTypeBuilder<ResourceParameterUsage> builder)
    {
        builder.ToTable("ResourceParameterUsages");

        // PK
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<ResourceParameterUsageId>())
            .ValueGeneratedNever();

        // ===== Resource FK =====
        builder.Property(x => x.ResourceId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.HasOne<AzureResource>()
            .WithMany()
            .HasForeignKey(x => x.ResourceId)
            .OnDelete(DeleteBehavior.Cascade); 

        // ===== ParameterDefinition FK =====
        builder.Property(x => x.ParameterId)
            .HasConversion(new IdValueConverter<ParameterDefinitionId>())
            .IsRequired();

        builder.HasOne<ParameterDefinition>()
            .WithMany()
            .HasForeignKey(x => x.ParameterId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== ParameterUsage (VO) =====
        builder.Property(x => x.Purpose)
            .HasConversion(new ParameterUsageConverter())
            .IsRequired();

        // ===== Invariant supplémentaire =====
        builder.HasIndex(x => new { x.ResourceId, x.ParameterId, x.Purpose })
            .IsUnique();
    }
}