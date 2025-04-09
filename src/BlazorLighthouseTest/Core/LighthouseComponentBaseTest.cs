using BlazorLighthouse.Core;
using BlazorLighthouseTest.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Moq;

namespace BlazorLighthouseTest.Core;

public partial class LighthouseComponentBaseTest
{
    [Fact]
    public async Task TestSetParametersAsync()
    {
        // arrange
        var parameter = new object();
        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>()
        {
            { nameof(TestComponent.Property), parameter }
        });

        var buildRenderTree = new Mock<Action>();
        var component = new TestComponent(buildRenderTree.Object);

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(component);

        buildRenderTree.Setup(obj => obj.Invoke())
            .Callback(() => Assert.Equal(parameter, component.Property));

        // act
        await rendererFake.Dispatcher.InvokeAsync(async () =>
            await component.SetParametersAsync(parameters));

        // assert
        Assert.Equal(parameter, component.Property);

        buildRenderTree.Verify(obj => obj.Invoke(), Times.Once);
    }
    
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
    
    [Fact]
    public async Task TestValueChanged()
    {
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

        // assert
        Assert.Equal(3, computedValue);
        Assert.Equal(1, computedRecalculationCount);
        buildRenderTree.Verify(obj => obj.Invoke(), Times.Never);
    }

    [Fact]
    public async Task TestNestedComputedValue_ValueChanged()
    {
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
        // TODO
    }

    [Fact]
    public async Task Test()
    {
        // arrange
        var value = 0;

        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);
        var signal3 = new Signal<int>(2);
        var tcs = new TaskCompletionSource();
        var tc2 = new TaskCompletionSource();
        var fc = 0;

        tcs.SetResult();

        var context = new SignalingContext();

        var buildRenderTree = new Mock<Action>();
        var component = new TestComponent(() =>
        {
            buildRenderTree.Object.Invoke();
            signal1.Get();
            signal2.Get();
            signal3.Get();
            tc2?.SetResult();
            tcs.Task.Wait();
            fc++;
            value = signal3.Get();
        });

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(component);

        await rendererFake.Dispatcher.InvokeAsync(
            component.ExecuteStateHasChanged);

        buildRenderTree.Invocations.Clear();

        // act

        tcs = new TaskCompletionSource();

        tc2 = new();
        var t1 = Task.Run(() => signal1.Set(2));
        await tc2.Task;

        var t2 = Task.Run(() => signal2.Set(3));
        while (!component!.IsRenderingQueued)
            ;

        signal3.Set(4);

        tc2 = null;
        tcs.SetResult();
        await t1;
        await t2;

        while (fc < 3)
            ;

        // assert
        buildRenderTree.Verify(obj => obj(), Times.Exactly(2));
        Assert.Equal(4, value);
    }

    internal class TestComponent(Action buildRenderTree) : LighthouseComponentBase
    {
        [Parameter]
        public object? Property { get; set; }

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
