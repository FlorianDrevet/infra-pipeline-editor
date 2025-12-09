using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InfraFlowSculptor.Domain.UserAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

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
        builder.Property(user => user.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => ResourceGroupId.Create(value)
            );
        
        builder.ComplexProperty(user => user.Name);
        
        builder.Property(order => order.Location)
            .IsRequired()
            .HasConversion(
                status => status.Value.ToString(),
                value => new Location(Enum.Parse<Location.LocationEnum>(value))
            );
        
        builder 
            .HasMany(p => p.Resources)
            .WithOne(t => t.ResourceGroup)
            .HasForeignKey(t => t.ResourceGroupId)
            .OnDelete(DeleteBehavior.Cascade); 
    }
}