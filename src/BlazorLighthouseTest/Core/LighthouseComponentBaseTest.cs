using BlazorLighthouse.Core;
using BlazorLighthouseTest.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Moq;

namespace BlazorLighthouseTest.Core;

public partial class LighthouseComponentBaseTest
{
    [Fact]
    public async Task TestInvokeAsync()
    {
        // arrange
        var buildRenderTree = new Mock<Action>();
        var component = new TestComponent(buildRenderTree.Object);

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(component);

        // act
        await component.ExecuteInvokeAsync(
            component.ExecuteStateHasChanged);

        // assert
        buildRenderTree.Verify(obj => obj.Invoke(), Times.Once);
    }

    [Fact]
    public async Task TestStateHasChanged()
    {
        // arrange
        var buildRenderTree = new Mock<Action>();
        var component = new TestComponent(buildRenderTree.Object);

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(component);

        // act
        await rendererFake.Dispatcher.InvokeAsync(
            component.ExecuteStateHasChanged);

        // assert
        buildRenderTree.Verify(obj => obj.Invoke(), Times.Once);
    }
    
    [Fact]
    public async Task TestStateHasChanged_MultipleComponentRedraws()
    {
        // arrange
        var innerBuildRenderTree = new Mock<Action>();
        var innerComponent = new TestComponent(() => innerBuildRenderTree.Object());

        var outerBuildRenderTree = new Mock<Action>();
        var outerComponent = new TestComponent(() =>
        {
            outerBuildRenderTree.Object();
            innerComponent.ExecuteStateHasChanged();
            innerComponent.ExecuteStateHasChanged();
        });

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(innerComponent);
        rendererFake.Attach(outerComponent);

        // act
        await rendererFake.Dispatcher.InvokeAsync(
            outerComponent.ExecuteStateHasChanged);

        // assert
        innerBuildRenderTree.Verify(obj => obj.Invoke(), Times.Once);
        outerBuildRenderTree.Verify(obj => obj.Invoke(), Times.Once);
    }
    
    internal class TestComponent(Action buildRenderTree) : LighthouseComponentBase
    {
        [Parameter]
        public object? Property1 { get; set; }
        [Parameter]
        public object? Property2 { get; set; }

        public Task ExecuteInvokeAsync(Action action)
        {
            return InvokeAsync(action);
        }

        public void ExecuteStateHasChanged()
        {
            StateHasChanged();
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);
            buildRenderTree();
        }
    }
}
