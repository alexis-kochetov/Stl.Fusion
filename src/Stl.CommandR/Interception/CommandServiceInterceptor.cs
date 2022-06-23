using Castle.DynamicProxy;
using Stl.CommandR.Internal;
using Stl.Interception.Interceptors;

namespace Stl.CommandR.Interception;

public class CommandServiceInterceptor : InterceptorBase
{
    public new class Options : InterceptorBase.Options
    { }

    protected ICommander Commander { get; }

    public CommandServiceInterceptor(
        Options options,
        IServiceProvider services,
        ILoggerFactory? loggerFactory = null)
        : base(options, services, loggerFactory)
        => Commander = services.GetRequiredService<ICommander>();

    protected override Action<IInvocation> CreateHandler<T>(
        IInvocation initialInvocation, MethodDef methodDef)
        => invocation => {
            var command = (ICommand) invocation.Arguments[0];
            var cancellationToken = (CancellationToken) invocation.Arguments[^1];
            var context = CommandContext.Current;
            if (ReferenceEquals(command, context?.UntypedCommand)) {
                // We're already inside the ICommander pipeline created for exactly this command
                invocation.Proceed();
                return;
            }

            // We're outside the ICommander pipeline, so we either have to block this call...
            if (!Commander.Options.AllowDirectCommandHandlerCalls)
                throw Errors.DirectCommandHandlerCallsAreNotAllowed();

            // Or route it via ICommander
            invocation.ReturnValue = methodDef.IsAsyncVoidMethod
                ? Commander.Call(command, cancellationToken)
                : Commander.Call((ICommand<T>)command, cancellationToken);
        };

    protected override MethodDef? CreateMethodDef(MethodInfo methodInfo, IInvocation initialInvocation)
    {
        try {
            var methodDef = new CommandHandlerMethodDef(this, methodInfo);
            return methodDef.IsValid ? methodDef : null;
        }
        catch {
            // CommandHandlerMethodDef may throw an exception,
            // which means methodDef isn't valid as well.
            return null;
        }
    }

    protected override void ValidateTypeInternal(Type type)
    {
        if (typeof(ICommandHandler).IsAssignableFrom(type))
            throw Errors.OnlyInterceptedCommandHandlersAllowed(type);
        var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic
            | BindingFlags.Instance | BindingFlags.Static
            | BindingFlags.FlattenHierarchy;
        foreach (var method in type.GetMethods(bindingFlags)) {
            var attr = MethodCommandHandler.GetAttribute(method);
            if (attr == null)
                continue;

            var methodDef = new CommandHandlerMethodDef(this, method);
            var attributeName = nameof(CommandHandlerAttribute)
#if NETSTANDARD2_0
                .Replace(nameof(Attribute), "");
#else
                .Replace(nameof(Attribute), "", StringComparison.Ordinal);
#endif
            if (!methodDef.IsValid) // attr.IsEnabled == false
                Log.Log(ValidationLogLevel,
                    "- {Method}: has [{Attribute}(false)]", method.ToString(), attributeName);
            else
                Log.Log(ValidationLogLevel,
                    "+ {Method}: [{Attribute}(" +
                    "Priority = {Priority}" +
                    ")]", method.ToString(), attributeName, attr.Priority);
        }
    }
}
