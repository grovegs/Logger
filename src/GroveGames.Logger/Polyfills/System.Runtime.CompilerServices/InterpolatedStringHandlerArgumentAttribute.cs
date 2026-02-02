#if !NET6_0_OR_GREATER

namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
internal sealed class InterpolatedStringHandlerArgumentAttribute : Attribute
{
    public InterpolatedStringHandlerArgumentAttribute(string argument)
    {
        Arguments = new[] { argument };
    }

    public InterpolatedStringHandlerArgumentAttribute(params string[] arguments)
    {
        Arguments = arguments;
    }

    public string[] Arguments { get; }
}

#endif
