using BlazorLighthouse.Core;
using BlazorLighthouse.Internal.Interfaces;

namespace BlazorLighthouse.Internal;

internal static class Lighthouse
{
    [ThreadStatic]
    private static Stack<TrackingToken>? stack;

    public static void Push(IRefreshable refreshable)
    {
        stack ??= [];
        stack.Push(new(refreshable));
    }

    public static void Register(Signal signal)
    {
        stack ??= [];
        if (stack.Count == 0)
            return;

        var token = stack.Peek();
        token.Signals.Add(signal);
        signal.RegisterRefreshable(token.Refreshable);
    }

    public static HashSet<Signal> Pop()
    {
        stack ??= [];

        var token = new TrackingToken(null!);
        if (stack.Count != 0)
            token = stack.Pop();
            
        return token.Signals;
    }

    private class TrackingToken(IRefreshable refreshable)
    {
        public IRefreshable Refreshable { get; } = refreshable;
        public HashSet<Signal> Signals { get; } = [];
    }
}
