using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.NetworkSecurityGroupAggregate;
using InfraFlowSculptor.Domain.NetworkSecurityGroupAggregate.Entities;
using InfraFlowSculptor.Domain.NetworkSecurityGroupAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="NetworkSecurityGroup"/> aggregate.</summary>
public class NetworkSecurityGroupConfiguration : IEntityTypeConfiguration<NetworkSecurityGroup>
{
    public void Configure(EntityTypeBuilder<NetworkSecurityGroup> builder)
    {
        builder.HasBaseType<AzureResource>()
            .ToTable("NetworkSecurityGroups");

        builder.HasMany(n => n.SecurityRules)
            .WithOne()
            .HasForeignKey(r => r.NetworkSecurityGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(n => n.SecurityRules)
            .HasField("_securityRules")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

/// <summary>EF Core configuration for the <see cref="NsgRule"/> entity.</summary>
public class NsgRuleConfiguration : IEntityTypeConfiguration<NsgRule>
{
    public void Configure(EntityTypeBuilder<NsgRule> builder)
    {
        builder.ToTable("NsgRules");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(new IdValueConverter<NsgRuleId>());

        builder.Property(r => r.NetworkSecurityGroupId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(r => r.Direction)
            .HasConversion(new EnumValueConverter<NsgDirection, NsgDirection.DirectionEnum>())
            .IsRequired();

        builder.Property(r => r.Access)
            .HasConversion(new EnumValueConverter<NsgAccess, NsgAccess.AccessEnum>())
            .IsRequired();

        builder.Property(r => r.Protocol)
            .HasConversion(new EnumValueConverter<NsgProtocol, NsgProtocol.ProtocolEnum>())
            .IsRequired();

        builder.Property(r => r.Name).IsRequired().HasMaxLength(260);
        builder.Property(r => r.Priority).IsRequired();
        builder.Property(r => r.SourceAddressPrefix).IsRequired().HasMaxLength(260);
        builder.Property(r => r.DestinationAddressPrefix).IsRequired().HasMaxLength(260);
        builder.Property(r => r.SourcePortRange).IsRequired().HasMaxLength(260);
        builder.Property(r => r.DestinationPortRange).IsRequired().HasMaxLength(260);

        builder.HasIndex(r => new { r.NetworkSecurityGroupId, r.Priority, r.Direction }).IsUnique();
    }
}
