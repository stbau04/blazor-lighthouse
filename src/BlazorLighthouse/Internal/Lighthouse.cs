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

    public static void Register(AbstractSignal signal)
    {
        InitializeTrackingTokens();
        if (trackingTokens.Count == 0)
            return;

        var trackingToken = trackingTokens.Peek();
        trackingToken.Signals.Add(signal);
        signal.RegisterRefreshable(trackingToken.Refreshable);
    }

    public static HashSet<AbstractSignal> Pop()
    {
        InitializeTrackingTokens();

        var trackingToken = new TrackingToken(IRefreshable.None);
        if (trackingTokens.Count != 0)
            trackingToken = trackingTokens.Pop();

        return trackingToken.Signals;
    }

    [MemberNotNull(nameof(trackingTokens))]
    private static void InitializeTrackingTokens()
    {
        trackingTokens ??= [];
    }

    private class TrackingToken(IRefreshable refreshable)
    {
        public IRefreshable Refreshable { get; } = refreshable;
        public HashSet<AbstractSignal> Signals { get; } = [];
    }
}
