namespace BlazorLighthouse.Core;

/// <summary>
/// Simple readonly value store that allows subscription to changes
/// </summary>
/// <typeparam name="T">Value type</typeparam>
public abstract class ReadonlySignal<T> : SignalBase
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
