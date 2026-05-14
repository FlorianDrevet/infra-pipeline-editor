using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.PrivateDnsZoneAggregate;
using InfraFlowSculptor.Domain.PrivateDnsZoneAggregate.Entities;
using InfraFlowSculptor.Domain.PrivateDnsZoneAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="PrivateDnsZone"/> aggregate.</summary>
public class PrivateDnsZoneConfiguration : IEntityTypeConfiguration<PrivateDnsZone>
{
    public void Configure(EntityTypeBuilder<PrivateDnsZone> builder)
    {
        builder.HasBaseType<AzureResource>()
            .ToTable("PrivateDnsZones");

        builder.HasMany(z => z.VirtualNetworkLinks)
            .WithOne()
            .HasForeignKey(l => l.PrivateDnsZoneId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(z => z.VirtualNetworkLinks)
            .HasField("_virtualNetworkLinks")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

/// <summary>EF Core configuration for the <see cref="VirtualNetworkLink"/> entity.</summary>
public class VirtualNetworkLinkConfiguration : IEntityTypeConfiguration<VirtualNetworkLink>
{
    public void Configure(EntityTypeBuilder<VirtualNetworkLink> builder)
    {
        builder.ToTable("VirtualNetworkLinks");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasConversion(new IdValueConverter<VirtualNetworkLinkId>());

        builder.Property(l => l.PrivateDnsZoneId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(l => l.VirtualNetworkId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(l => l.EnableAutoRegistration).IsRequired();
    }
}
