using BlazorLighthouse.Core;
using BlazorLighthouseTest.Types;
using Moq;

namespace BlazorLighthouseTest.Core;

public partial class LighthouseComponentBaseTest
{
    [Fact]
    public async Task TestValueChanged()
    {
        // arrange
        var signal = new Signal<int>(1);
        var siganlValue = 0;

        var buildRenderTree = new Mock<Action>();
        var component = new TestComponent(() =>
        {
            siganlValue = signal.Get();
            buildRenderTree.Object.Invoke();
        });

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(component);

        await rendererFake.Dispatcher.InvokeAsync(
            component.ExecuteStateHasChanged);

        buildRenderTree.Invocations.Clear();

        // act
        signal.Set(2);

        // assert
        Assert.Equal(2, siganlValue);
        buildRenderTree.Verify(obj => obj.Invoke(), Times.Once);
    }

    [Fact]
    public async Task TestValueChanged_MultipleComponentRedraws()
    {
        // arrange
        var signal = new Signal<int>(1);
        var siganlValue = 0;
        
        var innerBuildRenderTree = new Mock<Action>();
        var innerComponent = new TestComponent(() =>
        {
            innerBuildRenderTree.Object.Invoke();
            siganlValue = signal.Get();
        });

        var outerBuildRenderTree = new Mock<Action>();
        var outerComponent = new TestComponent(() =>
        {
            outerBuildRenderTree.Object.Invoke();

            signal.Set(2);
            signal.Set(3);
        });

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(innerComponent);
        rendererFake.Attach(outerComponent);

        await rendererFake.Dispatcher.InvokeAsync(
            innerComponent.ExecuteStateHasChanged);

        innerBuildRenderTree.Invocations.Clear();

        // act
        await rendererFake.Dispatcher.InvokeAsync(
            outerComponent.ExecuteStateHasChanged);

        // assert
        Assert.Equal(3, siganlValue);
        innerBuildRenderTree.Verify(obj => obj.Invoke(), Times.Once);
        outerBuildRenderTree.Verify(obj => obj.Invoke(), Times.Once);
    }

    [Fact]
    public async Task TestValueNotChanged()
    {
        // arrange
        var signal = new Signal<int>(1);
        var siganlValue = 0;

        var buildRenderTree = new Mock<Action>();
        var component = new TestComponent(() =>
        {
            siganlValue = signal.Get();
            buildRenderTree.Object.Invoke();
        });

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(component);

        await rendererFake.Dispatcher.InvokeAsync(
            component.ExecuteStateHasChanged);

        buildRenderTree.Invocations.Clear();

        // act
        signal.Set(1);

        // assert
        Assert.Equal(1, siganlValue);
        buildRenderTree.Verify(obj => obj.Invoke(), Times.Never);
    }

    [Fact]
    public async Task TestValueChangedMultipleTimes()
    {
        // arrange
        var signal = new Signal<int>(1);
        var siganlValue = 0;

        var buildRenderTree = new Mock<Action>();
        var component = new TestComponent(() =>
        {
            siganlValue = signal.Get();
            buildRenderTree.Object.Invoke();
        });

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(component);

        await rendererFake.Dispatcher.InvokeAsync(
            component.ExecuteStateHasChanged);

        buildRenderTree.Invocations.Clear();

        // act
        signal.Set(2);
        signal.Set(3);

        // assert
        Assert.Equal(3, siganlValue);
        buildRenderTree.Verify(obj => obj.Invoke(), Times.Exactly(2));
    }

    [Fact]
    public async Task TestUnreferencedValueChanged()
    {
        // arrange
        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);
        var siganlValue = 0;

        var buildRenderTree = new Mock<Action>();
        var component = new TestComponent(() =>
        {
            siganlValue = signal1.Get();
            buildRenderTree.Object.Invoke();
        });

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(component);

        await rendererFake.Dispatcher.InvokeAsync(
            component.ExecuteStateHasChanged);

        buildRenderTree.Invocations.Clear();

        // act
        signal2.Set(3);

        // assert
        Assert.Equal(1, siganlValue);
        buildRenderTree.Verify(obj => obj.Invoke(), Times.Never);
    }

    [Fact]
    public async Task TestUnreferencedValueChanged_WasReferenced()
    {
        // arrange
        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);
        var siganlValue = 0;

        var buildRenderTree = new Mock<Action>();
        var component = new TestComponent(() =>
        {
            buildRenderTree.Object.Invoke();
            if (signal1.Get() == 3)
            {
                siganlValue = signal1.Get();
                return;
            }

            siganlValue = signal2.Get();
        });

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(component);

        await rendererFake.Dispatcher.InvokeAsync(
            component.ExecuteStateHasChanged);

        buildRenderTree.Invocations.Clear();

        // act
        signal1.Set(3);
        signal2.Set(4);

        // assert
        Assert.Equal(3, siganlValue);
        buildRenderTree.Verify(obj => obj.Invoke(), Times.Once);
    }

    [Fact]
    public async Task TestNestedComputedValue()
    {
        // arrange
        var computedRecalculationCount = 0;
        var computedValue = 0;

        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);
        var computed = new Computed<int>(() => {
            computedRecalculationCount++;
            return signal1.Get();
        });

        var buildRenderTree = new Mock<Action>();
        var component = new TestComponent(() =>
        {
            computedValue = computed.Get() + signal2.Get();
            buildRenderTree.Object.Invoke();
        });

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(component);

        // act
        await rendererFake.Dispatcher.InvokeAsync(
            component.ExecuteStateHasChanged);

        // assert
        Assert.Equal(3, computedValue);
        Assert.Equal(1, computedRecalculationCount);
        buildRenderTree.Verify(obj => obj.Invoke(), Times.Once);
    }

    [Fact]
    public async Task TestNestedComputedValue_ValueChanged()
    {
        // arrange
        var computedRecalculationCount = 0;
        var computedValue = 0;

        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);
        var computed = new Computed<int>(() => {
            computedRecalculationCount++;
            return signal1.Get();
        });

        var buildRenderTree = new Mock<Action>();
        var component = new TestComponent(() =>
        {
            computedValue = computed.Get() + signal2.Get();
            buildRenderTree.Object.Invoke();
        });

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(component);

        await rendererFake.Dispatcher.InvokeAsync(
            component.ExecuteStateHasChanged);

        buildRenderTree.Invocations.Clear();

        // act
        signal2.Set(3);

        // assert
        Assert.Equal(4, computedValue);
        Assert.Equal(1, computedRecalculationCount);
        buildRenderTree.Verify(obj => obj.Invoke(), Times.Once);
    }

    [Fact]
    public async Task TestNestedComputedValue_NestedValueChanged()
    {
        // arrange
        var computedRecalculationCount = 0;
        var computedValue = 0;

        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);
        var computed = new Computed<int>(() => {
            computedRecalculationCount++;
            return signal1.Get();
        });

        var buildRenderTree = new Mock<Action>();
        var component = new TestComponent(() =>
        {
            computedValue = computed.Get() + signal2.Get();
            buildRenderTree.Object.Invoke();
        });

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(component);

        await rendererFake.Dispatcher.InvokeAsync(
            component.ExecuteStateHasChanged);

        buildRenderTree.Invocations.Clear();

        // act
        signal1.Set(3);

        // assert
        Assert.Equal(5, computedValue);
        Assert.Equal(2, computedRecalculationCount);
        buildRenderTree.Verify(obj => obj.Invoke(), Times.Once);
    }

    [Fact]
    public async Task TestNestedComputedValueChanged_MultipleComponentRedraws()
    {
        // arrange
        var signal = new Signal<int>(1);
        var computed = new Computed<int>(signal.Get);
        var computedValue = 0;

        var innerBuildRenderTree = new Mock<Action>();
        var innerComponent = new TestComponent(() =>
        {
            innerBuildRenderTree.Object.Invoke();
            computedValue = computed.Get();
        });

        var outerBuildRenderTree = new Mock<Action>();
        var outerComponent = new TestComponent(() =>
        {
            outerBuildRenderTree.Object.Invoke();

            signal.Set(2);
            signal.Set(3);
        });

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(innerComponent);
        rendererFake.Attach(outerComponent);

        await rendererFake.Dispatcher.InvokeAsync(
            innerComponent.ExecuteStateHasChanged);

        innerBuildRenderTree.Invocations.Clear();

        // act
        await rendererFake.Dispatcher.InvokeAsync(
            outerComponent.ExecuteStateHasChanged);

        // assert
        Assert.Equal(3, computedValue);
        innerBuildRenderTree.Verify(obj => obj.Invoke(), Times.Once);
        outerBuildRenderTree.Verify(obj => obj.Invoke(), Times.Once);
    }

    [Fact]
    public async Task TestComponentDisposed()
    {
        // arrange
        var signal = new Signal<int>(1);
        var siganlValue = 0;

        var buildRenderTree = new Mock<Action>();
        var component = new TestComponent(() =>
        {
            siganlValue = signal.Get();
            buildRenderTree.Object.Invoke();
        });

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(component);

        await rendererFake.Dispatcher.InvokeAsync(
            component.ExecuteStateHasChanged);

        buildRenderTree.Invocations.Clear();

        // act
        component.Dispose();
        signal.Set(2);

        // assert
        Assert.Equal(1, siganlValue);
        buildRenderTree.Verify(obj => obj.Invoke(), Times.Never);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await rendererFake.Dispatcher.InvokeAsync(
                component.ExecuteStateHasChanged));
    }
    
    [Fact]
    public async Task TestReferencedSignalDisposed()
    {
        // arrange
        var context = new SignalingContext();

        var signal = new Signal<int>(context, 1);
        var siganlValue = 0;

        var buildRenderTree = new Mock<Action>();
        var component = new TestComponent(() =>
        {
            siganlValue = signal.Get();
            buildRenderTree.Object.Invoke();
        });

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(component);

        await rendererFake.Dispatcher.InvokeAsync(
            component.ExecuteStateHasChanged);

        buildRenderTree.Invocations.Clear();

        // act
        context.Dispose();

        // assert
        Assert.Throws<InvalidOperationException>(
            () => signal.Set(2));

        Assert.Equal(1, siganlValue);
        buildRenderTree.Verify(obj => obj(), Times.Never);
    }

    [Fact]
    public async Task TestMultipleSignalChangesAtOnce()
    {
        // arrange
        var recalculationCount = 0;
        var value = 0;

        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);
        var signal3 = new Signal<int>(3);

        var taskCompletionSource1 = new TaskCompletionSource();
        var taskCompletionSource2 = new TaskCompletionSource();

        taskCompletionSource1.SetResult();

        var context = new SignalingContext();
        var buildRenderTree = new Mock<Action>();
        var component = new TestComponent(() =>
        {
            buildRenderTree.Object.Invoke();

            signal1.Get();
            signal2.Get();
            signal3.Get();

            taskCompletionSource2.SetResult();
            taskCompletionSource1.Task.Wait();

            recalculationCount++;
            value = signal3.Get();
        });

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(component);

        await rendererFake.Dispatcher.InvokeAsync(
            component.ExecuteStateHasChanged);

        buildRenderTree.Invocations.Clear();

        // act
        taskCompletionSource1 = new();
        taskCompletionSource2 = new();

        var setterTask1 = Task.Run(() => signal1.Set(4));
        await taskCompletionSource2.Task;

        var setterTask2 = Task.Run(() => signal2.Set(5));
        while (!component!.HasPendingQueuedRender)
            ;

        signal3.Set(6);

        taskCompletionSource2 = new();
        taskCompletionSource1.SetResult();

        await setterTask1;
        await setterTask2;

        while (recalculationCount < 3)
            ;

        // assert
        Assert.Equal(6, value);
        buildRenderTree.Verify(obj => obj(), Times.Exactly(2));
    }
}
