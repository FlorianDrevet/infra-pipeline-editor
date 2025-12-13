using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Domain.Models;

namespace Shared.Infrastructure.Persistence.Configurations.Extensions;

public static class EntityTypeBuilderExtensions
{
    public static PropertyBuilder<TId> ConfigureAggregateRootId<TEntity, TId>(
        this EntityTypeBuilder<TEntity> builder)
        where TEntity : AggregateRoot<TId>
        where TId : Id<TId>
    {
        return builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasConversion(new IdValueConverter<TId>());
    }
}