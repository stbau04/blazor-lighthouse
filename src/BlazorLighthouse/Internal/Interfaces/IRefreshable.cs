using BlazorLighthouse.Core;
using System.Diagnostics.CodeAnalysis;

namespace BlazorLighthouse.Internal.Interfaces;

internal interface IRefreshable
{
    public static IRefreshable None { get; } = new EmptyRefreshable();

    internal void Refresh();
    internal void Dispose(AbstractSignal signal);

    [ExcludeFromCodeCoverage]
    public class EmptyRefreshable : IRefreshable
    {
        void IRefreshable.Refresh()
        {

        }

        void IRefreshable.Dispose(AbstractSignal signal)
        {

        }
    }
}
