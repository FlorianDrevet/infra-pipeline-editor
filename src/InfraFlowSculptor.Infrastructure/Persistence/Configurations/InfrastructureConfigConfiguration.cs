using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

public class InfrastructureConfigConfiguration : IEntityTypeConfiguration<InfrastructureConfig>
{
    public void Configure(EntityTypeBuilder<InfrastructureConfig> builder)
    {
        ConfigureUsersTable(builder);
    }

    private void ConfigureUsersTable(EntityTypeBuilder<InfrastructureConfig> builder)
    {
        builder.ToTable(nameof(InfrastructureConfig));
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => InfrastructureConfigId.Create(value)
            );
        
        builder 
            .HasMany(p => p.ResourceGroups)
            .WithOne(t => t.InfraConfig)
            .HasForeignKey(t => t.InfraConfigId)
            .OnDelete(DeleteBehavior.Cascade); 
        
        builder.ComplexProperty(user => user.Name);
    }
}