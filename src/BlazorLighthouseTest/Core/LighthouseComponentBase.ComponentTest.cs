using BlazorLighthouseTest.Types;
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
}
