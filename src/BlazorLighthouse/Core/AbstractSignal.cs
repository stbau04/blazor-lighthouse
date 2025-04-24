using BlazorLighthouse.Internal.Interfaces;

namespace BlazorLighthouse.Core;

/// <summary>
/// Base class for all types of signals
/// </summary>
public abstract class AbstractSignal : IContextDisposable
{
    /// <summary>
    /// Signaling context for the current signal
    /// </summary>
    protected readonly SignalingContext context;

    private HashSet<IRefreshable> refreshables = [];

    internal AbstractSignal(SignalingContext context)
    {
        this.context = context;
        context.RegisterContextDisposable(this);
    }

    /// <summary>
    /// Notify the signaling system that the value has chagned
    /// </summary>
    internal protected void ValueHasChanged()
    {
        var currentRefreshables = refreshables;
        refreshables = [];

        Refresh(currentRefreshables);
    }

    internal void RegisterRefreshable(IRefreshable refreshable)
    {
        lock (context.LockObject)
        {
            RegisterRefreshableSynchronized(refreshable);
        }
    }

    internal void UnregisterRefreshable(IRefreshable refreshable)
    {
        refreshables.Remove(refreshable);
    }
    
    private void RegisterRefreshableSynchronized(IRefreshable refreshable)
    {
        context.AssertIsNotDisposed();
        refreshables.Add(refreshable);
    }

    void IContextDisposable.Dispose()
    {
        foreach (var refreshable in refreshables)
            refreshable.Dispose(this);

        refreshables.Clear();
    } 

    private static void Refresh(ISet<IRefreshable> refreshables)
    {
        foreach (var refreshable in refreshables)
            refreshable.Refresh();
    }
}
