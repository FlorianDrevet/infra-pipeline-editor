using InfraFlowSculptor.Domain.Common.BaseModels.Entites;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using Shared.Infrastructure.Persistence.Configurations.Converters;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

public sealed class InputOutputLinkConfiguration
    : IEntityTypeConfiguration<InputOutputLink>
{
    public void Configure(EntityTypeBuilder<InputOutputLink> builder)
    {
        builder.ToTable("ResourceLinks", t => 
            t.HasCheckConstraint("CK_ResourceLinks_SourceTarget_Different",
                "[SourceResourceId] <> [TargetResourceId]"));

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<InputOutputId>())
            .ValueGeneratedNever();

        builder.Property(x => x.OutputType)
            .HasConversion<int>();

        builder.Property(x => x.InputType)
            .HasConversion<int>();

        // Source resource
        builder.HasOne(x => x.SourceResource)
            .WithMany(r => r.Outputs)
            .HasForeignKey(x => x.SourceResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Target resource
        builder.HasOne(x => x.TargetResource)
            .WithMany(r => r.Inputs)
            .HasForeignKey(x => x.TargetResourceId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(x => new
        {
            x.SourceResourceId,
            x.TargetResourceId,
            x.OutputType,
            x.InputType
        }).IsUnique();
    }
}