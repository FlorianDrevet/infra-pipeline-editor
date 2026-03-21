using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using Shared.Infrastructure.Persistence.Configurations.Converters;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="ProjectMember"/> entity.</summary>
public sealed class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    private const string TableName = "ProjectMembers";

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<ProjectMemberId>());

        builder.Property(x => x.UserId)
            .HasConversion(new IdValueConverter<UserId>())
            .IsRequired();

        builder.Property(x => x.Role)
            .HasConversion(new EnumValueConverter<ProjectRole, ProjectRole.ProjectRoleEnum>())
            .IsRequired();

        builder.Property(x => x.ProjectId)
            .HasConversion(new IdValueConverter<ProjectId>())
            .IsRequired();

        builder.HasOne(pm => pm.User)
            .WithMany()
            .HasForeignKey(pm => pm.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
