using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Stl.Conversion.Internal;

namespace Stl.Conversion;

public static class ServiceCollectionExt
{
    public static IServiceCollection AddConverters(
        this IServiceCollection services,
#if NET5_0_OR_GREATER        
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
        Type? sourceConverterProviderGenericType = null)
    {
        sourceConverterProviderGenericType ??= typeof(DefaultSourceConverterProvider<>);
        services.TryAddSingleton<IConverterProvider, DefaultConverterProvider>();
        services.TryAddSingleton(typeof(ISourceConverterProvider<>), sourceConverterProviderGenericType);
        return services;
    }
}
