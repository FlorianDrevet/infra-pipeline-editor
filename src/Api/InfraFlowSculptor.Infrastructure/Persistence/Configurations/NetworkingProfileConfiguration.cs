using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.NetworkingProfileAggregate;
using InfraFlowSculptor.Domain.NetworkingProfileAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="NetworkingProfile"/> aggregate.</summary>
public sealed class NetworkingProfileConfiguration : IEntityTypeConfiguration<NetworkingProfile>
{
    public void Configure(EntityTypeBuilder<NetworkingProfile> builder)
    {
        builder.ToTable("NetworkingProfiles");
        builder.HasKey(x => x.Id);

        builder.ConfigureAggregateRootId<NetworkingProfile, NetworkingProfileId>();

        builder.Property<uint>("xmin")
            .HasColumnType("xid")
            .IsRowVersion();

        // ── FK to InfrastructureConfig (1:0..1) ────────────────────────────────────
        builder.Property(x => x.InfraConfigId)
            .HasConversion(new IdValueConverter<InfrastructureConfigId>())
            .IsRequired();

        builder.HasIndex(x => x.InfraConfigId)
            .IsUnique();

        builder.HasOne<Domain.InfrastructureConfigAggregate.InfrastructureConfig>()
            .WithMany()
            .HasForeignKey(x => x.InfraConfigId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Mode ───────────────────────────────────────────────────────────────────
        builder.Property(x => x.Mode)
            .HasConversion(new EnumValueConverter<NetworkingMode, NetworkingMode.Mode>())
            .HasMaxLength(20)
            .IsRequired();

        // ── VnetReference (Owned) ──────────────────────────────────────────────────
        builder.OwnsOne(x => x.VnetReference, vnet =>
        {
            vnet.Property(v => v.Source)
                .HasColumnName("VnetSource")
                .HasConversion(new EnumValueConverter<VnetSource, VnetSource.SourceType>())
                .HasMaxLength(20)
                .IsRequired();

            vnet.Property(v => v.ExistingVnetResourceId)
                .HasColumnName("VnetExistingResourceId")
                .HasMaxLength(500);

            vnet.Property(v => v.CreateNewAddressSpace)
                .HasColumnName("VnetCreateNewAddressSpace")
                .HasConversion(
                    c => c != null ? c.Value : null,
                    s => s != null ? new CidrBlock(s) : null)
                .HasMaxLength(50);

            vnet.Property(v => v.PrivateEndpointsSubnetName)
                .HasColumnName("VnetPeSubnetName")
                .HasMaxLength(260)
                .IsRequired();

            vnet.Property(v => v.PrivateEndpointsSubnetAddressPrefix)
                .HasColumnName("VnetPeSubnetAddressPrefix")
                .HasConversion(
                    c => c != null ? c.Value : null,
                    s => s != null ? new CidrBlock(s) : null)
                .HasMaxLength(50);
        });

        // ── DnsConfig (Owned) ──────────────────────────────────────────────────────
        builder.OwnsOne(x => x.DnsConfig, dns =>
        {
            dns.Property(d => d.Mode)
                .HasColumnName("DnsMode")
                .HasConversion(new EnumValueConverter<DnsMode, DnsMode.Mode>())
                .HasMaxLength(20)
                .IsRequired();

            dns.Property(d => d.HubResourceGroupId)
                .HasColumnName("DnsHubResourceGroupId")
                .HasMaxLength(500);

            dns.Property(d => d.HubSubscriptionId)
                .HasColumnName("DnsHubSubscriptionId")
                .HasMaxLength(100);
        });

        // ── Environment Overrides ──────────────────────────────────────────────────
        builder.HasMany(x => x.EnvironmentOverrides)
            .WithOne()
            .HasForeignKey(o => o.NetworkingProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.EnvironmentOverrides)
            .HasField("_environmentOverrides")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
