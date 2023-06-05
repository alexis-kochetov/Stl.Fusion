using System.Diagnostics.CodeAnalysis;

namespace Stl.Conversion.Internal;

public class CastToBaseConverter<
#if NET5_0_OR_GREATER        
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
    TSource,
#if NET5_0_OR_GREATER        
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
    TTarget> : Converter<TSource, TTarget>
    where TSource : TTarget
{
    public static CastToBaseConverter<TSource, TTarget> Instance { get; } = new();

    public override TTarget Convert(TSource source)
        => source;
    public override object? ConvertUntyped(object? source)
        => source;

    public override Option<TTarget> TryConvert(TSource source)
        => source;
    public override Option<object?> TryConvertUntyped(object? source)
        => (object?) (TTarget?) source;
}
