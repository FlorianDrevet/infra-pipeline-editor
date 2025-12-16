using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using Shared.Infrastructure.Persistence.Configurations.Converters;
using Shared.Infrastructure.Persistence.Configurations.Extensions;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

public class AzureResourceConfiguration : IEntityTypeConfiguration<AzureResource>
{
    public void Configure(EntityTypeBuilder<AzureResource> builder)
    {
        ConfigureUsersTable(builder);
    }

    private void ConfigureUsersTable(EntityTypeBuilder<AzureResource> builder)
    {
        builder.ToTable(nameof(AzureResource));
        builder.HasKey(user => user.Id);
        
        builder.ConfigureAggregateRootId<AzureResource, AzureResourceId>();
        
        builder.Property(order => order.Location)
            .IsRequired()
            .HasConversion(new EnumValueConverter<Location, Location.LocationEnum>());
        
        //builder.HasBaseType<AzureResource>();
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasConversion(new SingleValueConverter<Name, string>());

        // Configuration de la relation DependsOn
        builder.HasMany(r => r.DependsOn)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "AzureResourceDependencies",
                j => j.HasOne<AzureResource>()
                    .WithMany()
                    .HasForeignKey("DependsOnId")
                    .OnDelete(DeleteBehavior.Restrict),
                j => j.HasOne<AzureResource>()
                    .WithMany()
                    .HasForeignKey("ResourceId")
                    .OnDelete(DeleteBehavior.Cascade));
        
        builder.Navigation(r => r.DependsOn)
            .HasField("_dependsOn")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}