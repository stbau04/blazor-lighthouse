using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.Logging;
using Moq;

namespace BlazorLighthouseTest.Types;

#pragma warning disable BL0006
internal class RendererFake(
    IServiceProvider serviceProvider,
    ILoggerFactory loggerFactory) 
        : Renderer(serviceProvider, loggerFactory)
{
    public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();

    public void Attach(IComponent component)
    {
        AssignRootComponentId(component);
    }

    protected override void HandleException(Exception exception)
    {
        throw exception;
    }

    protected override Task UpdateDisplayAsync(in RenderBatch renderBatch)
    {
        return Task.CompletedTask;
    }

    public static RendererFake Create()
    {
        var serviceProvider = new Mock<IServiceProvider>();
        var loggerFactory = LoggerFactory.Create(builder => { });
        return new(serviceProvider.Object, loggerFactory);
    }
}
#pragma warning restore BL0006
