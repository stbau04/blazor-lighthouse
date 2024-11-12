using BlazorLighthouse.Internal;
using BlazorLighthouse.Internal.Interfaces;

namespace BlazorLighthouse.Core;

/// <summary>
/// Base class for all types of signals
/// </summary>
public abstract class Signal : IContextDisposable
{
    /// <summary>
    /// Signaling context for the current signal
    /// </summary>
    protected readonly SignalingContext context;

    private HashSet<IRefreshable> refreshables = [];

    internal Signal(SignalingContext context)
    {
        this.context = context;
        context?.RegisterContextDisposable(this);
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
        refreshables.Clear();
        foreach (var refreshable in refreshables)
            refreshable.Dispose(this);
    }

    private static void Refresh(ISet<IRefreshable> refreshables)
    {
        foreach (var refreshable in refreshables)
            refreshable.Refresh();
    }
}

/// <summary>
/// Simple readonly value store that allows subscription to changes
/// </summary>
/// <typeparam name="T">Value type</typeparam>
public abstract class ReadonlySignal<T> : Signal
{
    internal ReadonlySignal(SignalingContext context)
        : base(context)
    {

    }

    /// <summary>
    /// Get the current value stored and registers available subscribers to changes
    /// </summary>
    /// <returns>The current value stored</returns>
    public abstract T Get();
}

/// <summary>
/// Simple value store that allows subscription to changes
/// </summary>
/// <typeparam name="T">Value type</typeparam>
public sealed class Signal<T> : ReadonlySignal<T>
{
    private readonly object lockObject = new();

    private T value;

    /// <summary>
    /// Instantiate a new signal that belongs to no context
    /// </summary>
    /// <param name="value">Initial signal value</param>
    public Signal(T value) : this(null, value)
    {

    }

    /// <summary>
    /// Instantiate a new signal that belongs to the specified context
    /// </summary>
    /// <param name="context">Context to define lifespan</param>
    /// <param name="value">Initial signal value</param>
    public Signal(SignalingContext? context, T value)
        : base(context ?? new())
    {
        this.value = value;
    }

    /// <inheritdoc/>
    public override T Get()
    {
        lock (lockObject)
        {
            return GetSynchronized();
        }
    }

    /// <summary>
    /// Sets the stored value and notifies all current subscribers that changes happened
    /// </summary>
    /// <param name="value">The new value to store</param>
    public void Set(T value)
    {
        lock (lockObject)
        {
            SetSynchronized(value);
        }
    }

    private T GetSynchronized()
    {
        context.AssertIsNotDisposed();
        Lighthouse.Register(this);
        return value;
    }

    private void SetSynchronized(T value)
    {
        context.AssertIsNotDisposed();
        if (Equals(this.value, value))
            return;

        this.value = value;
        ValueHasChanged();
    }
}
