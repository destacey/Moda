using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Moda.Infrastructure.Persistence.Extensions;

internal static class ModelBuilderExtensions
{
    public static ModelBuilder AppendGlobalQueryFilter<TInterface>(this ModelBuilder modelBuilder, Expression<Func<TInterface, bool>> filter)
    {
        //get a list of entities without a baseType that implement the interface TInterface
        var entities = modelBuilder.Model.GetEntityTypes()
            .Where(e => e.BaseType is null && e.ClrType.GetInterface(typeof(TInterface).Name) is not null)
            .Select(e => e.ClrType)
            .Where(t => typeof(TInterface).IsAssignableFrom(t));

        foreach (var entity in entities)
        {
            var parameterType = Expression.Parameter(modelBuilder.Entity(entity).Metadata.ClrType);
            var filterBody = ReplacingExpressionVisitor.Replace(filter.Parameters.Single(), parameterType, filter.Body);

            // apply the query filter with a key based on the interface type
            modelBuilder.Entity(entity).Metadata.SetQueryFilter(
                typeof(TInterface).Name,
                Expression.Lambda(filterBody, parameterType));
        }

        return modelBuilder;
    }
}