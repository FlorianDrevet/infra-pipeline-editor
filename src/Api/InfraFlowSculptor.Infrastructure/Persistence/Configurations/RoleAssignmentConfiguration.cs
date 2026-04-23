using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.Entites;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

public sealed class RoleAssignmentConfiguration : IEntityTypeConfiguration<RoleAssignment>
{
    public void Configure(EntityTypeBuilder<RoleAssignment> builder)
    {
        builder.ToTable("RoleAssignments");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(new IdValueConverter<RoleAssignmentId>())
            .ValueGeneratedNever();

        builder.Property(r => r.SourceResourceId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(r => r.TargetResourceId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(r => r.ManagedIdentityType)
            .HasConversion(new EnumValueConverter<ManagedIdentityType, ManagedIdentityType.IdentityTypeEnum>())
            .IsRequired();

        builder.Property(r => r.RoleDefinitionId)
            .IsRequired();

        builder.Property(r => r.UserAssignedIdentityId)
            .HasConversion(
                v => v == null ? (Guid?)null : v.Value,
                v => v.HasValue ? new AzureResourceId(v.Value) : null)
            .IsRequired(false);

        builder.HasOne<AzureResource>()
            .WithMany()
            .HasForeignKey(r => r.TargetResourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AzureResource>()
            .WithMany()
            .HasForeignKey(r => r.UserAssignedIdentityId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Audit DB-005 (2026-04-23): prevent duplicate functional role assignments
        // (same source → same target → same identity → same role definition).
        // NOTE: existing duplicates must be cleaned up before applying this migration.
        builder.HasIndex(r => new
            {
                r.SourceResourceId,
                r.TargetResourceId,
                r.UserAssignedIdentityId,
                r.RoleDefinitionId
            })
            .IsUnique();
    }
}