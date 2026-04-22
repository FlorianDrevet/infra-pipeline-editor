using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Extensions;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

public class AzureResourceConfiguration : IEntityTypeConfiguration<AzureResource>
{
    public void Configure(EntityTypeBuilder<AzureResource> builder)
    {
        ConfigureUsersTable(builder);
    }

    private static void ConfigureUsersTable(EntityTypeBuilder<AzureResource> builder)
    {
        builder.ToTable(nameof(AzureResource));
        builder.HasKey(user => user.Id);
        
        builder.ConfigureAggregateRootId<AzureResource, AzureResourceId>();
        
        builder.Property(order => order.Location)
            .IsRequired()
            .HasConversion(new EnumValueConverter<Location, Location.LocationEnum>());
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasConversion(new SingleValueConverter<Name, string>());

        builder.Property(x => x.CustomNameOverride)
            .IsRequired(false)
            .HasMaxLength(260);

        builder.Property(x => x.IsExisting)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.ResourceType)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.ResourceType);

        builder.Property(x => x.AssignedUserAssignedIdentityId)
            .HasConversion(
                v => v == null ? (Guid?)null : v.Value,
                v => v.HasValue ? new AzureResourceId(v.Value) : null)
            .IsRequired(false);

        builder.HasOne<AzureResource>()
            .WithMany()
            .HasForeignKey(r => r.AssignedUserAssignedIdentityId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Configuration de la relation DependsOn
        builder.HasMany(r => r.DependsOn)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "AzureResourceDependencies",
                j => j.HasOne<AzureResource>()
                    .WithMany()
                    .HasForeignKey("DependsOnId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<AzureResource>()
                    .WithMany()
                    .HasForeignKey("ResourceId")
                    .OnDelete(DeleteBehavior.Cascade));
        
        builder.Navigation(r => r.DependsOn)
            .HasField("_dependsOn")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        
        builder.HasMany(r => r.ParameterUsages)
            .WithOne()
            .HasForeignKey(x => x.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.RoleAssignments)
            .WithOne()
            .HasForeignKey(r => r.SourceResourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(r => r.RoleAssignments)
            .HasField("_roleAssignments")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(r => r.AppSettings)
            .WithOne()
            .HasForeignKey(s => s.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(r => r.AppSettings)
            .HasField("_appSettings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(r => r.SecureParameterMappings)
            .WithOne()
            .HasForeignKey(m => m.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(r => r.SecureParameterMappings)
            .HasField("_secureParameterMappings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(r => r.CustomDomains)
            .WithOne()
            .HasForeignKey(cd => cd.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(r => r.CustomDomains)
            .HasField("_customDomains")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}