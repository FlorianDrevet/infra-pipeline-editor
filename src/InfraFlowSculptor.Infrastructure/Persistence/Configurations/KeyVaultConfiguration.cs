using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

public class KeyVaultConfiguration : IEntityTypeConfiguration<AzureResource>
{
    public void Configure(EntityTypeBuilder<AzureResource> builder)
    {
        ConfigureUsersTable(builder);
    }

    private void ConfigureUsersTable(EntityTypeBuilder<AzureResource> builder)
    {
        builder.ToTable(nameof(KeyVault));
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => AzureResourceId.Create(value)
            );
        
        builder.Property(order => order.Location)
            .IsRequired()
            .HasConversion(
                status => (int)status.Value,
                value => new Location((Location.LocationEnum)value)
            );
        
        //builder.HasBaseType<AzureResource>();
        
        builder.ComplexProperty(user => user.Name);
    }
}