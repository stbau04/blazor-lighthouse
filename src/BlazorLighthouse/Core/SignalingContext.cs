using BlazorLighthouse.Internal.Interfaces;
namespace BlazorLighthouse.Core;

/// <summary>
/// Defines a scope for the existance of signals. Required if objects with shorter lifetime subscribe to objects with a longer one.
/// Important: If not used properly memory leaks can and will happen.
/// </summary>
public class SignalingContext : IDisposable
{
    private readonly List<IContextDisposable> contextDisposables = [];

    private bool isDisposed = false;

    internal object LockObject { get; } = new();

    /// <summary>
    /// Dispose the signaling context. Enforces clean up of all signaling ressources allocated by object referencing this context.
    /// </summary>
    public virtual void Dispose()
    {
        lock (LockObject) 
        {
            DisposeSynchronized();
        }
    }

    internal void RegisterContextDisposable(IContextDisposable accessTracker)
    {
        lock (LockObject) 
        {
            RegisterContextDisposableSynchronized(accessTracker);
        }
    }

    internal void AssertIsNotDisposed()
    {
        if (isDisposed)
            throw new InvalidOperationException("Context is already disposed");
    }

    private void DisposeSynchronized()
    {
        isDisposed = true;
        contextDisposables.ForEach(
            contextDisposable => contextDisposable.Dispose());
    }

    private void RegisterContextDisposableSynchronized(IContextDisposable contextDisposable)
    {
        AssertIsNotDisposed();
        contextDisposables.Add(contextDisposable);
    }
}
