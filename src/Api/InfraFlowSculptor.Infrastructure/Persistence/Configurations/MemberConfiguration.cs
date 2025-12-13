using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using Shared.Infrastructure.Persistence.Configurations.Converters;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

public sealed class MemberConfiguration
    : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("infrastructureconfig_members");

        builder.HasKey(pm => pm.Id);

        builder.Property(pm => pm.Id)
            .HasConversion(new IdValueConverter<MemberId>())
            .ValueGeneratedNever();

        builder.Property(pm => pm.UserId)
            .HasConversion(new IdValueConverter<UserId>())
            .IsRequired();

        builder.Property(pm => pm.Role)
            .HasConversion(new EnumValueConverter<Role, Role.RoleEnum>())
            .IsRequired();

        builder.HasIndex(pm => new { pm.InfraConfigId, pm.UserId })
            .IsUnique();
    }
}
