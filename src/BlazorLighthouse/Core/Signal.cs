using BlazorLighthouse.Internal;

namespace BlazorLighthouse.Core;

/// <summary>
/// Simple value store that allows subscription to changes
/// </summary>
/// <typeparam name="T">Value type</typeparam>
public sealed class Signal<T> : ReadonlySignal<T>
{
    private readonly Lock lockObject = new();

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
