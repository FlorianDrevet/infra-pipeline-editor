using InfraFlowSculptor.Domain.NetworkingProfileAggregate.Entities;
using InfraFlowSculptor.Domain.NetworkingProfileAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="NetworkingProfileEnvironmentOverride"/> entity.</summary>
public sealed class NetworkingProfileEnvironmentOverrideConfiguration
    : IEntityTypeConfiguration<NetworkingProfileEnvironmentOverride>
{
    public void Configure(EntityTypeBuilder<NetworkingProfileEnvironmentOverride> builder)
    {
        builder.ToTable("NetworkingProfileEnvironmentOverrides");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<NetworkingProfileEnvironmentOverrideId>());

        builder.Property(x => x.NetworkingProfileId)
            .HasConversion(new IdValueConverter<NetworkingProfileId>())
            .IsRequired();

        builder.Property(x => x.EnvironmentName)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => new { x.NetworkingProfileId, x.EnvironmentName })
            .IsUnique();

        // ── VnetReferenceOverride (Owned, nullable) ────────────────────────────────
        builder.OwnsOne(x => x.VnetReferenceOverride, vnet =>
        {
            vnet.Property(v => v.Source)
                .HasColumnName("VnetSourceOverride")
                .HasConversion(new EnumValueConverter<VnetSource, VnetSource.SourceType>())
                .HasMaxLength(20);

            vnet.Property(v => v.ExistingVnetResourceId)
                .HasColumnName("VnetExistingResourceIdOverride")
                .HasMaxLength(500);

            vnet.Property(v => v.CreateNewAddressSpace)
                .HasColumnName("VnetCreateNewAddressSpaceOverride")
                .HasConversion(
                    c => c != null ? c.Value : null,
                    s => s != null ? new CidrBlock(s) : null)
                .HasMaxLength(50);

            vnet.Property(v => v.PrivateEndpointsSubnetName)
                .HasColumnName("VnetPeSubnetNameOverride")
                .HasMaxLength(260);

            vnet.Property(v => v.PrivateEndpointsSubnetAddressPrefix)
                .HasColumnName("VnetPeSubnetAddressPrefixOverride")
                .HasConversion(
                    c => c != null ? c.Value : null,
                    s => s != null ? new CidrBlock(s) : null)
                .HasMaxLength(50);
        });

        // ── DnsConfigOverride (Owned, nullable) ────────────────────────────────────
        builder.OwnsOne(x => x.DnsConfigOverride, dns =>
        {
            dns.Property(d => d.Mode)
                .HasColumnName("DnsModeOverride")
                .HasConversion(new EnumValueConverter<DnsMode, DnsMode.Mode>())
                .HasMaxLength(20);

            dns.Property(d => d.HubResourceGroupId)
                .HasColumnName("DnsHubResourceGroupIdOverride")
                .HasMaxLength(500);

            dns.Property(d => d.HubSubscriptionId)
                .HasColumnName("DnsHubSubscriptionIdOverride")
                .HasMaxLength(100);
        });
    }
}
