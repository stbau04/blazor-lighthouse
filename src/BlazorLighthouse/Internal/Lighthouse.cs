using BlazorLighthouse.Core;
using BlazorLighthouse.Internal.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace BlazorLighthouse.Internal;

internal static class Lighthouse
{
    [ThreadStatic]
    private static Stack<TrackingToken>? trackingTokens;

    public static void Push(IRefreshable refreshable)
    {
        InitializeTrackingTokens();
        trackingTokens.Push(new(refreshable));
    }

    public static void Register(SignalBase signal)
    {
        InitializeTrackingTokens();
        if (trackingTokens.Count == 0)
            return;

        var token = trackingTokens.Peek();
        token.Signals.Add(signal);
        signal.RegisterRefreshable(token.Refreshable);
    }

    public static HashSet<SignalBase> Pop()
    {
        InitializeTrackingTokens();

        var token = new TrackingToken(IRefreshable.None);
        if (trackingTokens.Count != 0)
            token = trackingTokens.Pop();

        return token.Signals;
    }

    [MemberNotNull(nameof(trackingTokens))]
    private static void InitializeTrackingTokens()
    {
        trackingTokens ??= [];
    }

    private class TrackingToken(IRefreshable refreshable)
    {
        public IRefreshable Refreshable { get; } = refreshable;
        public HashSet<SignalBase> Signals { get; } = [];
    }
}
