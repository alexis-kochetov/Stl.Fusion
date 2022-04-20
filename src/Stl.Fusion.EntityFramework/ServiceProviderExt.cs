using Microsoft.EntityFrameworkCore;

namespace Stl.Fusion.EntityFramework;

public static class ServiceProviderExt
{
    public static DbHub<TDbContext> DbHub<TDbContext>(this IServiceProvider services)
        where TDbContext : DbContext
        => services.GetRequiredService<DbHub<TDbContext>>();

    public static IDbEntityResolver<TKey, TDbEntity> DbEntityResolver<TKey, TDbEntity>(this IServiceProvider services)
        where TKey : notnull
        where TDbEntity : class
        => services.GetRequiredService<IDbEntityResolver<TKey, TDbEntity>>();

    public static IDbEntityConverter<TDbEntity, TEntity> DbEntityConverter<TDbEntity, TEntity>(this IServiceProvider services)
        where TEntity : notnull
        where TDbEntity : class
        => services.GetRequiredService<IDbEntityConverter<TDbEntity, TEntity>>();
}
