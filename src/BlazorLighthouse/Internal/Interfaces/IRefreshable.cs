using BlazorLighthouse.Core;
using System.Diagnostics.CodeAnalysis;

namespace BlazorLighthouse.Internal.Interfaces;

internal interface IRefreshable
{
    public static IRefreshable None { get; } = new EmptyRefreshable();

    internal void Refresh();
    internal void Dispose(SignalBase signal);

    [ExcludeFromCodeCoverage]
    public class EmptyRefreshable : IRefreshable
    {
        void IRefreshable.Dispose(SignalBase signal)
        {

        }

        void IRefreshable.Refresh()
        {

        }
    }
}
