using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.VirtualNetworkAggregate;
using InfraFlowSculptor.Domain.VirtualNetworkAggregate.Entities;
using InfraFlowSculptor.Domain.VirtualNetworkAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="VirtualNetwork"/> aggregate.</summary>
public class VirtualNetworkConfiguration : IEntityTypeConfiguration<VirtualNetwork>
{
    public void Configure(EntityTypeBuilder<VirtualNetwork> builder)
    {
        builder.HasBaseType<AzureResource>()
            .ToTable("VirtualNetworks");

        builder.Property(v => v.EnableDdosProtection)
            .IsRequired();

        builder.HasMany(v => v.Subnets)
            .WithOne()
            .HasForeignKey(s => s.VirtualNetworkId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(v => v.Subnets)
            .HasField("_subnets")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(v => v.EnvironmentSettings)
            .WithOne()
            .HasForeignKey(es => es.VirtualNetworkId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(v => v.EnvironmentSettings)
            .HasField("_environmentSettings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

/// <summary>EF Core configuration for the <see cref="Subnet"/> entity.</summary>
public class SubnetConfiguration : IEntityTypeConfiguration<Subnet>
{
    public void Configure(EntityTypeBuilder<Subnet> builder)
    {
        builder.ToTable("Subnets");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasConversion(new IdValueConverter<SubnetId>());

        builder.Property(s => s.VirtualNetworkId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(s => s.Name)
            .HasConversion(new SingleValueConverter<Name, string>())
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(s => s.Delegation)
            .HasConversion(new NullableEnumValueConverter<SubnetDelegation, SubnetDelegation.SubnetDelegationEnum>());

        builder.Property(s => s.PrivateEndpointNetworkPolicies)
            .HasConversion(new EnumValueConverter<PrivateEndpointNetworkPolicy, PrivateEndpointNetworkPolicy.PolicyEnum>())
            .IsRequired();

        builder.Property(s => s.NsgId)
            .HasConversion(new NullableIdValueConverter<AzureResourceId>());

        builder.Property(s => s.ServiceEndpoints)
            .HasColumnType("jsonb");
    }
}

/// <summary>EF Core configuration for the <see cref="VirtualNetworkEnvironmentSettings"/> entity.</summary>
public class VirtualNetworkEnvironmentSettingsConfiguration : IEntityTypeConfiguration<VirtualNetworkEnvironmentSettings>
{
    public void Configure(EntityTypeBuilder<VirtualNetworkEnvironmentSettings> builder)
    {
        builder.ToTable("VirtualNetworkEnvironmentSettings");

        builder.HasKey(es => es.Id);

        builder.Property(es => es.Id)
            .HasConversion(new IdValueConverter<VirtualNetworkEnvironmentSettingsId>());

        builder.Property(es => es.VirtualNetworkId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(es => es.EnvironmentName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(es => es.AddressSpaces)
            .HasColumnType("jsonb");

        builder.Property(es => es.DnsServers)
            .HasColumnType("jsonb");
    }
}
