using BlazorLighthouse.Core;

namespace BlazorLighthouse.Internal.Interfaces;

internal interface IRefreshable
{
    internal void Refresh();
    internal void Dispose(Signal signal);
}
