using BlazorLighthouse.Internal;
using BlazorLighthouse.Internal.Interfaces;

namespace BlazorLighthouse.Core;

/// <summary>
/// Calculates values based on other signal values. Result gets updated when those values change
/// </summary>
/// <typeparam name="T">Result type</typeparam>
public sealed class Computed<T> : ReadonlySignal<T>, IRefreshable
{
    private readonly Func<T> valueProvider;
    private readonly AccessTracker accessTracker;
    private readonly Lazy<Signal<T>> lazySignal;
    private readonly Lock lockObject = new();

    internal bool IsEvaluationQueued { get; private set; } = false;

    /// <summary>
    /// Instantiate a new computed value that belongs to no context
    /// </summary>
    /// <param name="valueProvider">Provider for the computed value</param>
    public Computed(Func<T> valueProvider) : this(null, valueProvider)
    {

    }

    /// <summary>
    /// Instantiate a new computed value that belongs to the specified context
    /// </summary>
    /// <param name="context">Context to define lifespan</param>
    /// <param name="valueProvider">Provider for the computed value</param>
    public Computed(SignalingContext? context, Func<T> valueProvider)
        : base(context ?? new())
    {
        this.valueProvider = valueProvider;
        accessTracker = new(this, context);

        lazySignal = new(() => new(EvaluateValueProvider()));
        _ = lazySignal.Value;
    }

    /// <inheritdoc/>
    public override T Get()
    {
        context.AssertIsNotDisposed();
        return lazySignal.Value.Get();
    }

    private T EvaluateValueProvider()
    {
        return accessTracker.Track(() => {
            IsEvaluationQueued = false;
            return valueProvider();
        });
    }

    private bool SetEvaluationQueued()
    {
        lock (lockObject)
        {
            return SetEvaluationQueuedSync();
        }
    }

    private bool SetEvaluationQueuedSync()
    {
        if (IsEvaluationQueued)
            return false;

        IsEvaluationQueued = true;
        return true;
    }

    void IRefreshable.Refresh()
    {
        if (!SetEvaluationQueued())
            return;

        lazySignal.Value.Set(EvaluateValueProvider());
    }

    void IRefreshable.Dispose(SignalBase signal)
    {
        accessTracker.Untrack(signal);
    }
}
