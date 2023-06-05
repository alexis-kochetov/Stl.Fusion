using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Stl.Fusion.Blazor;

#if NET5_0_OR_GREATER
[UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2026:RequiresUnreferencedCode",
     Justification = "NullDispatcher.Instance is already used by the framework")]
#endif
public static class DispatcherExt
{
    private static readonly Dispatcher? NullDispatcher = null;
    
#if NET5_0_OR_GREATER
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicFields, 
        typeName: "Microsoft.AspNetCore.Components.WebAssembly.Rendering.NullDispatcher", 
        assemblyName: "Microsoft.AspNetCore.Components.WebAssembly")]
    [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2026:RequiresUnreferencedCode",
        Justification = "NullDispatcher.Instance is already used by the framework.")]
    [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2072:ArgumentDoesNotSatisfyAnnotation",
        Justification = "NullDispatcher.Instance handled by dynamic dependency attribute above.")]
#endif
    static DispatcherExt()
    {
        var assembly = typeof(WebAssemblyHost).Assembly;
        var tNullDispatcher = assembly.GetType("Microsoft.AspNetCore.Components.WebAssembly.Rendering.NullDispatcher");
        
        NullDispatcher = tNullDispatcher == null
            ? null
            : GetNullDispatcherInstance(tNullDispatcher);
    }

    public static bool IsNullDispatcher(this Dispatcher dispatcher)
        => ReferenceEquals(dispatcher, NullDispatcher);

#if NET5_0_OR_GREATER
    [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2070:ParameterDoesNotHaveMatchingAnnotation",
        Justification = "NullDispatcher.Instance handled by dynamic dependency attribute above.")]
#endif
    private static Dispatcher? GetNullDispatcherInstance(
#if NET5_0_OR_GREATER        
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)]
#endif
        IReflect tNullDispatcher)
    {
        var fInstance = tNullDispatcher.GetField("Instance", BindingFlags.Static | BindingFlags.Public);
        return (Dispatcher?)fInstance?.GetValue(null);
    }
}
