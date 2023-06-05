using System.Diagnostics.CodeAnalysis;

namespace Stl.Conversion.Internal;

public class FuncConverter<
#if NET5_0_OR_GREATER        
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
    TSource, 
#if NET5_0_OR_GREATER        
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
    TTarget> : Converter<TSource, TTarget>
{
    public Func<TSource, TTarget> Converter { get; init; }
    public Func<TSource, Option<TTarget>> TryConverter { get; init; }

    public override TTarget Convert(TSource source)
        => Converter(source);
    public override object? ConvertUntyped(object? source)
        => Converter((TSource) source!);

    public override Option<TTarget> TryConvert(TSource source)
        => TryConverter(source).Cast<TTarget>();
    public override Option<object?> TryConvertUntyped(object? source)
        => source is TSource t ? TryConverter(t).Cast<object?>() : Option<object?>.None;

    public FuncConverter(
        Func<TSource, TTarget> converter,
        Func<TSource, Option<TTarget>> tryConverter)
    {
        Converter = converter;
        TryConverter = tryConverter;
    }
}

public static class FuncConverter<TSource>
{
    public static FuncConverter<TSource, TTarget> New<TTarget>(Func<TSource, TTarget> converter)
        => new(converter, ToTryConvert(converter));
    public static FuncConverter<TSource, TTarget> New<TTarget>(
        Func<TSource, Option<TTarget>> tryConverter,
        Func<TSource, TTarget>? converter)
        => new(converter ?? FromTryConvert(tryConverter), tryConverter);

    public static Func<TSource, TTarget> FromTryConvert<TTarget>(Func<TSource, Option<TTarget>> converter)
        => s => {
            var targetOpt = converter(s);
            return targetOpt.HasValue
                ? targetOpt.ValueOrDefault!
                : throw Errors.CantConvert(typeof(TSource), typeof(TTarget));
        };

    public static Func<TSource, Option<TTarget>> ToTryConvert<TTarget>(Func<TSource, TTarget> converter)
        => s => {
            try {
                return converter(s);
            }
            catch {
                // Intended
                return Option<TTarget>.None;
            }
        };
}
