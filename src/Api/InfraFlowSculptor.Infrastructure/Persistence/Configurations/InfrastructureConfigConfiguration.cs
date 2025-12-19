using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using Shared.Infrastructure.Persistence.Configurations.Converters;
using Shared.Infrastructure.Persistence.Configurations.Extensions;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

public class InfrastructureConfigConfiguration : IEntityTypeConfiguration<InfrastructureConfig>
{
    public void Configure(EntityTypeBuilder<InfrastructureConfig> builder)
    {
        builder.ToTable(nameof(InfrastructureConfig));
        builder.HasKey(user => user.Id);
        
        builder.ConfigureAggregateRootId<InfrastructureConfig,  InfrastructureConfigId>();
        
        builder 
            .HasMany(p => p.ResourceGroups)
            .WithOne(t => t.InfraConfig)
            .HasForeignKey(t => t.InfraConfigId)
            .OnDelete(DeleteBehavior.Cascade); 
        
        builder.Property(config => config.Name)
            .HasConversion(new SingleValueConverter<Name, string>());
        
        
        builder
            .HasMany(m => m.Members)
            .WithOne(t => t.InfraConfig)
            .HasForeignKey(t => t.InfraConfigId)
            .OnDelete(DeleteBehavior.Cascade);
        
        ConfigureEnvironmentsTable(builder);
    }

    private void ConfigureEnvironmentsTable(EntityTypeBuilder<InfrastructureConfig> builder)
    {
        builder.OwnsMany(config => config.EnvironmentDefinitions, env =>
        {
            env.ToTable("Environments");
            env.HasKey(e => e.Id);
            env.Property(e => e.Name).HasConversion(new SingleValueConverter<Name, string>());
            env.Property(e => e.Prefix).HasConversion(new SingleValueConverter<Prefix, string>());
            env.Property(e => e.Suffix).HasConversion(new SingleValueConverter<Suffix, string>());
            env.Property(e => e.SubscriptionId).HasConversion(new SingleValueConverter<SubscriptionId, Guid>());
            env.Property(e => e.TenantId).HasConversion(new SingleValueConverter<TenantId, Guid>());
            env.Property(e => e.Location).HasConversion(new EnumValueConverter<Location, Location.LocationEnum>());
            env.Property(e => e.RequiresApproval).HasConversion(new SingleValueConverter<RequiresApproval, bool>());
            env.Property(e => e.Order).HasConversion(new SingleValueConverter<Order, int>());
        });
    }
}