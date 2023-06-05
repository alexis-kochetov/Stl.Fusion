using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components;
#if NET5_0_OR_GREATER 
using System.Diagnostics.CodeAnalysis;
#endif

namespace Stl.Fusion.Blazor;

public sealed class ComponentInfo
{
    private static readonly ConcurrentDictionary<Type, ComponentInfo> ComponentInfoCache = new();

    public Type Type { get; }
    public bool HasCustomParameterComparers { get; }
    public IReadOnlyDictionary<string, ComponentParameterInfo> Parameters { get; }

    public static ComponentInfo Get(
#if NET5_0_OR_GREATER        
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        Type componentType)
        => ComponentInfoCache.GetOrAdd(componentType,
            static (
#if NET5_0_OR_GREATER        
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
                componentType1) => new ComponentInfo(componentType1));
    
    private ComponentInfo(
#if NET5_0_OR_GREATER        
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        Type type)
    {
        if (!typeof(IComponent).IsAssignableFrom(type))
            throw new ArgumentOutOfRangeException(nameof(type));

        var parameterComparerProvider = ParameterComparerProvider.Instance;
        var bindingFlags = BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public;
        var parameters = new Dictionary<string, ComponentParameterInfo>(StringComparer.Ordinal);
        var hasCustomParameterComparers = false;
        foreach (var property in type.GetProperties(bindingFlags)) {
            var pa = property.GetCustomAttribute<ParameterAttribute>(true);
            CascadingParameterAttribute? cpa = null;
            if (pa == null) {
                cpa = property.GetCustomAttribute<CascadingParameterAttribute>(true);
                if (cpa == null)
                    continue; // Not a parameter
            }

            var comparer = parameterComparerProvider.Get(property);
            hasCustomParameterComparers |= comparer is not DefaultParameterComparer;
            var parameter = new ComponentParameterInfo() {
                Property = property,
                IsCascading = cpa != null,
                IsCapturingUnmatchedValues = pa?.CaptureUnmatchedValues ?? false,
                CascadingParameterName = cpa?.Name,
                Getter = property.GetGetter<IComponent, object>(true),
                Setter = property.GetSetter<IComponent, object>(true),
                Comparer = comparer,
            };
            parameters.Add(parameter.Property.Name, parameter);
        }
        Type = type;
        Parameters = new ReadOnlyDictionary<string, ComponentParameterInfo>(parameters);
        HasCustomParameterComparers = hasCustomParameterComparers;
    }

    public bool ShouldSetParameters(ComponentBase component, ParameterView parameterView)
    {
        if (!HasCustomParameterComparers)
            return true; // No custom comparers -> trigger default flow

        var parameters = Parameters;
        foreach (var parameterValue in parameterView) {
            if (!parameters.TryGetValue(parameterValue.Name, out var parameterInfo))
                return true; // Unknown parameter -> trigger default flow

            var oldValue = parameterInfo.Getter.Invoke(component);
            if (!parameterInfo.Comparer.AreEqual(oldValue, parameterValue.Value))
                return true; // Comparer says values aren't equal -> trigger default flow
        }

        return false; // All parameter values are equal
    }
}
