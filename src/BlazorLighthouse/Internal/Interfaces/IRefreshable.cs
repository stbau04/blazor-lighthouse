using BlazorLighthouse.Core;
using System.Diagnostics.CodeAnalysis;

namespace BlazorLighthouse.Internal.Interfaces;

internal interface IRefreshable
{
    public static IRefreshable Default { get; } = new DefaultRefreshable();

    internal void Refresh();
    internal void Dispose(SignalBase signal);

    [ExcludeFromCodeCoverage]
    public class DefaultRefreshable : IRefreshable
    {
        void IRefreshable.Dispose(SignalBase signal)
        {

        }

        void IRefreshable.Refresh()
        {

        }
    }
}
