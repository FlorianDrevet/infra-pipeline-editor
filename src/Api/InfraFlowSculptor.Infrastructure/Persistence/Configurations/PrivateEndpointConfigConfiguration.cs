using InfraFlowSculptor.Domain.Common.BaseModels.Entites;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="PrivateEndpointConfig"/> entity.</summary>
public class PrivateEndpointConfigConfiguration : IEntityTypeConfiguration<PrivateEndpointConfig>
{
    public void Configure(EntityTypeBuilder<PrivateEndpointConfig> builder)
    {
        builder.ToTable("PrivateEndpointConfigs");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(new IdValueConverter<PrivateEndpointConfigId>());

        builder.Property(p => p.ResourceId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(p => p.SubnetId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(p => p.GroupId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.AutoApproval)
            .IsRequired();

        builder.Property(p => p.PrivateDnsZoneId)
            .HasConversion(new NullableIdValueConverter<AzureResourceId>());

        builder.Property(p => p.CustomNetworkInterfaceName)
            .HasMaxLength(260);
    }
}
