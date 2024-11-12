using BlazorLighthouse.Core;
using BlazorLighthouse.Internal.Interfaces;

namespace BlazorLighthouse.Internal;

internal class AccessTracker : IContextDisposable
{
    private readonly IRefreshable refreshable;
    private readonly SignalingContext context;

    private HashSet<Signal> signals = [];

    public AccessTracker(IRefreshable refreshable, SignalingContext? context)
    {
        this.refreshable = refreshable;
        this.context = context ?? new();

        context?.RegisterContextDisposable(this);
    }

    public void Track(Action action)
    {
        _ = Track<object?>(() =>
        {
            action();
            return null;
        });
    }
    
    public T Track<T>(Func<T> func)
    {
        lock (context.LockObject)
        {
            return TrackSynchronized(func);
        }
    }

    public void Untrack(Signal signal)
    {
        lock (context.LockObject)
        {
            UntrackSyncrhonized(signal);
        }
    }

    public void Dispose()
    {
        Untrack();
    }

    private void Untrack()
    {
        foreach (var signal in signals)
            signal.UnregisterRefreshable(refreshable);
    }

    private T TrackSynchronized<T>(Func<T> func)
    {
        Untrack();
        context.AssertIsNotDisposed();

        Lighthouse.Push(refreshable);
        var value = func();
        signals = Lighthouse.Pop();

        return value;
    }

    private void UntrackSyncrhonized(Signal signal)
    {
        signals.Remove(signal);
    }
}
