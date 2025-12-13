using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations.Extensions;

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
        
        builder.ConfigureAggregateRootId<InfrastructureConfig,  InfrastructureConfigId>();
        
        builder 
            .HasMany(p => p.ResourceGroups)
            .WithOne(t => t.InfraConfig)
            .HasForeignKey(t => t.InfraConfigId)
            .OnDelete(DeleteBehavior.Cascade); 
        
        builder.ComplexProperty(user => user.Name);
        
        builder
            .HasMany(m => m.Members)
            .WithOne(t => t.InfraConfig)
            .HasForeignKey(t => t.InfraConfigId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation("_members")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}