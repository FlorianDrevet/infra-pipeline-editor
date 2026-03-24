using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for <see cref="ProjectMember"/>.</summary>
public sealed class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        builder.ToTable("project_members");

        builder.HasKey(pm => pm.Id);

        builder.Property(pm => pm.Id)
            .HasConversion(new IdValueConverter<ProjectMemberId>())
            .ValueGeneratedNever();

        builder.Property(pm => pm.UserId)
            .HasConversion(new IdValueConverter<UserId>())
            .IsRequired();

        builder.Property(pm => pm.Role)
            .HasConversion(new EnumValueConverter<Role, Role.RoleEnum>())
            .IsRequired();

        builder.HasIndex(pm => new { pm.ProjectId, pm.UserId })
            .IsUnique();

        builder.HasOne(pm => pm.User)
            .WithMany()
            .HasForeignKey(pm => pm.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}