using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.UserAssignedIdentityAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="UserAssignedIdentity"/> aggregate (TPT).
/// </summary>
public class UserAssignedIdentityConfiguration : IEntityTypeConfiguration<UserAssignedIdentity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UserAssignedIdentity> builder)
    {
        builder.HasBaseType<AzureResource>()
            .ToTable("UserAssignedIdentities");
    }
}
