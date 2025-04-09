using BlazorLighthouse.Core;
using BlazorLighthouse.Internal.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace BlazorLighthouse.Internal;

internal static class Lighthouse
{
    [ThreadStatic]
    private static Stack<TrackingToken>? stack;

    public static void Push(IRefreshable refreshable)
    {
        InitializeThreadStatic();
        stack.Push(new(refreshable));
    }

    public static void Register(SignalBase signal)
    {
        InitializeThreadStatic();
        if (stack.Count == 0)
            return;

        var token = stack.Peek();
        token.Signals.Add(signal);
        signal.RegisterRefreshable(token.Refreshable);
    }

    public static HashSet<SignalBase> Pop()
    {
        InitializeThreadStatic();

        var token = new TrackingToken(IRefreshable.Default);
        if (stack.Count != 0)
            token = stack.Pop();

        return token.Signals;
    }

    [MemberNotNull(nameof(stack))]
    private static void InitializeThreadStatic()
    {
        stack ??= [];
    }

    private class TrackingToken(IRefreshable refreshable)
    {
        public IRefreshable Refreshable { get; } = refreshable;
        public HashSet<SignalBase> Signals { get; } = [];
    }
}
