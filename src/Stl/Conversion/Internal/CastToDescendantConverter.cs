using System.Diagnostics.CodeAnalysis;

namespace Stl.Conversion.Internal;

public class CastToDescendantConverter<
#if NET5_0_OR_GREATER        
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
    TSource, 
#if NET5_0_OR_GREATER        
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
    TTarget> : Converter<TSource, TTarget>
    where TTarget : TSource
{
    public static CastToDescendantConverter<TSource, TTarget> Instance { get; } = new();

    public override TTarget Convert(TSource source)
        => (TTarget) source!;
    public override object? ConvertUntyped(object? source)
        => (TTarget) source!;

    public override Option<TTarget> TryConvert(TSource source)
        => (TTarget) source!;
    public override Option<object?> TryConvertUntyped(object? source)
        => (object?) (TTarget?) source;
}
