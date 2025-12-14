using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using Shared.Infrastructure.Persistence.Configurations.Converters;
using Shared.Infrastructure.Persistence.Configurations.Extensions;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

public class ResourceGroupConfiguration : IEntityTypeConfiguration<ResourceGroup>
{
    public void Configure(EntityTypeBuilder<ResourceGroup> builder)
    {
        ConfigureUsersTable(builder);
    }

    private void ConfigureUsersTable(EntityTypeBuilder<ResourceGroup> builder)
    {
        builder.ToTable(nameof(ResourceGroup));
        builder.HasKey(user => user.Id);
        
        builder.ConfigureAggregateRootId<ResourceGroup, ResourceGroupId>();
        
        builder.Property(config => config.Name)
            .HasConversion(new SingleValueConverter<Name, string>());
        
        builder.Property(order => order.Location)
            .IsRequired()
            .HasConversion(new EnumValueConverter<Location, Location.LocationEnum>());
        
        builder 
            .HasMany(p => p.Resources)
            .WithOne(t => t.ResourceGroup)
            .HasForeignKey(t => t.ResourceGroupId)
            .OnDelete(DeleteBehavior.Cascade); 
    }
}