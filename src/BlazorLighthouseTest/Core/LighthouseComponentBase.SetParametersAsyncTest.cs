using BlazorLighthouse.Core;
using BlazorLighthouseTest.Types;
using Microsoft.AspNetCore.Components;
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
            { nameof(TestComponent.Property1), parameter }
        });

        var buildRenderTree = new Mock<Action>();
        var component = new TestComponent(buildRenderTree.Object);

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(component);

        buildRenderTree.Setup(obj => obj.Invoke())
            .Callback(() => Assert.Equal(parameter, component.Property1));

        // act
        await rendererFake.Dispatcher.InvokeAsync(async () =>
            await component.SetParametersAsync(parameters));

        // assert
        Assert.Equal(parameter, component.Property1);
        buildRenderTree.Verify(obj => obj.Invoke(), Times.Once);
    }
    
    [Fact]
    public async Task TestSetParametersAsync_SignalParameter()
    {
        // arrange
        var parameter = new Signal<int>(0);
        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>()
        {
            { nameof(TestComponent.Property1), parameter }
        });

        var buildRenderTree = new Mock<Action>();
        var component = new TestComponent(buildRenderTree.Object);

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(component);

        buildRenderTree.Setup(obj => obj.Invoke())
            .Callback(() => Assert.Equal(parameter, component.Property1));

        // act
        await rendererFake.Dispatcher.InvokeAsync(async () =>
            await component.SetParametersAsync(parameters));

        // assert
        Assert.Equal(parameter, component.Property1);
        buildRenderTree.Verify(obj => obj.Invoke(), Times.Once);
    }
    
    [Fact]
    public async Task TestSetParametersAsync_NoParameters()
    {
        // arrange
        var buildRenderTree = new Mock<Action>();
        var component = new TestComponent(buildRenderTree.Object);

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(component);

        buildRenderTree.Setup(obj => obj.Invoke())
            .Callback(() => Assert.Null(component.Property1));

        // act
        await rendererFake.Dispatcher.InvokeAsync(async () =>
            await component.SetParametersAsync(ParameterView.Empty));

        // assert
        Assert.Null(component.Property1);
        buildRenderTree.Verify(obj => obj.Invoke(), Times.Once);
    }

    [Fact]
    public async Task TestSetParametersAsync_MultipleTimes()
    {
        // arrange
        var parameter = new object();
        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>()
        {
            { nameof(TestComponent.Property1), parameter }
        });

        var buildRenderTree = new Mock<Action>();
        var component = new TestComponent(buildRenderTree.Object);

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(component);

        buildRenderTree.Setup(obj => obj.Invoke())
            .Callback(() => Assert.Equal(parameter, component.Property1));

        await rendererFake.Dispatcher.InvokeAsync(async () =>
            await component.SetParametersAsync(parameters));

        // act
        await rendererFake.Dispatcher.InvokeAsync(async () =>
            await component.SetParametersAsync(parameters));

        // assert
        Assert.Equal(parameter, component.Property1);
        buildRenderTree.Verify(obj => obj.Invoke(), Times.Exactly(2));
    }

    [Fact]
    public async Task TestSetParametersAsync_MultipleTimesWithNewParameter()
    {
        // arrange1
        var parameter = new Signal<int>(0);
        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>()
        {
            { nameof(TestComponent.Property1), parameter }
        });

        var buildRenderTree = new Mock<Action>();
        var component = new TestComponent(buildRenderTree.Object);

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(component);

        await rendererFake.Dispatcher.InvokeAsync(async () =>
            await component.SetParametersAsync(ParameterView.Empty));

        buildRenderTree.Setup(obj => obj.Invoke())
            .Callback(() => Assert.Equal(parameter, component.Property1));

        // act
        await rendererFake.Dispatcher.InvokeAsync(async () =>
            await component.SetParametersAsync(parameters));

        // assert
        Assert.Equal(parameter, component.Property1);
        buildRenderTree.Verify(obj => obj.Invoke(), Times.Exactly(2));
    }

    [Fact]
    public async Task TestSetParametersAsync_MultipleTimesWithRemovedParameter()
    {
        // arrange
        var parameter = new Signal<int>(0);
        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>()
        {
            { nameof(TestComponent.Property1), parameter }
        });

        var buildRenderTree = new Mock<Action>();
        var component = new TestComponent(buildRenderTree.Object);

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(component);

        buildRenderTree.Setup(obj => obj.Invoke())
            .Callback(() => Assert.Equal(parameter, component.Property1));

        await rendererFake.Dispatcher.InvokeAsync(async () =>
            await component.SetParametersAsync(parameters));

        // act
        await rendererFake.Dispatcher.InvokeAsync(async () =>
            await component.SetParametersAsync(ParameterView.Empty));

        // assert
        Assert.Equal(parameter, component.Property1);
        buildRenderTree.Verify(obj => obj.Invoke(), Times.Once);
    }

    [Fact]
    public async Task TestSetParametersAsync_MultipleTimesWithSignalParameter()
    {
        // arrange
        var parameter = new Signal<int>(0);
        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>()
        {
            { nameof(TestComponent.Property1), parameter }
        });

        var buildRenderTree = new Mock<Action>();
        var component = new TestComponent(buildRenderTree.Object);

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(component);

        buildRenderTree.Setup(obj => obj.Invoke())
            .Callback(() => Assert.Equal(parameter, component.Property1));

        await rendererFake.Dispatcher.InvokeAsync(async () =>
            await component.SetParametersAsync(parameters));

        // act
        await rendererFake.Dispatcher.InvokeAsync(async () =>
            await component.SetParametersAsync(parameters));

        // assert
        Assert.Equal(parameter, component.Property1);
        buildRenderTree.Verify(obj => obj.Invoke(), Times.Once);
    }

    [Fact]
    public async Task TestSetParametersAsync_MultipleTimesWithChangedSignalParameter()
    {
        // arrange
        var parameter = new Signal<int>(0);
        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>()
        {
            { nameof(TestComponent.Property1), parameter }
        });

        var buildRenderTree = new Mock<Action>();
        var component = new TestComponent(buildRenderTree.Object);

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(component);

        buildRenderTree.Setup(obj => obj.Invoke())
            .Callback(() => Assert.Equal(parameter, component.Property1));

        await rendererFake.Dispatcher.InvokeAsync(async () =>
            await component.SetParametersAsync(parameters));

        // act
        parameter = new Signal<int>(0);
        parameters = ParameterView.FromDictionary(new Dictionary<string, object?>()
        {
            { nameof(TestComponent.Property1), parameter }
        });

        await rendererFake.Dispatcher.InvokeAsync(async () =>
            await component.SetParametersAsync(parameters));

        // assert
        Assert.Equal(parameter, component.Property1);
        buildRenderTree.Verify(obj => obj.Invoke(), Times.Exactly(2));
    }

    [Fact]
    public async Task TestSetParametersAsync_MultipleTimesWithNoParameters()
    {
        // arrange
        var buildRenderTree = new Mock<Action>();
        var component = new TestComponent(buildRenderTree.Object);

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(component);

        buildRenderTree.Setup(obj => obj.Invoke())
            .Callback(() => Assert.Null(component.Property1));

        await rendererFake.Dispatcher.InvokeAsync(async () =>
            await component.SetParametersAsync(ParameterView.Empty));

        // act
        await rendererFake.Dispatcher.InvokeAsync(async () =>
            await component.SetParametersAsync(ParameterView.Empty));

        // assert
        Assert.Null(component.Property1);
        buildRenderTree.Verify(obj => obj.Invoke(), Times.Once);
    }

    [Fact]
    public async Task TestSetParametersAsync_MultipleTimesWithMultipleParameters()
    {
        // arrange
        var parameter1 = new object();
        var parameter2 = new Signal<int>(0);
        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>()
        {
            { nameof(TestComponent.Property1), parameter1 },
            { nameof(TestComponent.Property2), parameter2 }
        });

        var buildRenderTree = new Mock<Action>();
        var component = new TestComponent(buildRenderTree.Object);

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(component);

        buildRenderTree.Setup(obj => obj.Invoke())
            .Callback(() =>
            {
                Assert.Equal(parameter1, component.Property1);
                Assert.Equal(parameter2, component.Property2);
            });

        await rendererFake.Dispatcher.InvokeAsync(async () =>
            await component.SetParametersAsync(parameters));

        // act
        await rendererFake.Dispatcher.InvokeAsync(async () =>
            await component.SetParametersAsync(parameters));

        // assert
        Assert.Equal(parameter1, component.Property1);
        Assert.Equal(parameter2, component.Property2);

        buildRenderTree.Verify(obj => obj.Invoke(), Times.Exactly(2));
    }

    [Fact]
    public async Task TestSetParametersAsync_MultipleTimesWithMultipleSignalParameters()
    {
        // arrange
        var parameter1 = new Signal<int>(0);
        var parameter2 = new Signal<int>(0);
        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>()
        {
            { nameof(TestComponent.Property1), parameter1 },
            { nameof(TestComponent.Property2), parameter2 }
        });

        var buildRenderTree = new Mock<Action>();
        var component = new TestComponent(buildRenderTree.Object);

        var rendererFake = RendererFake.Create();
        rendererFake.Attach(component);

        buildRenderTree.Setup(obj => obj.Invoke())
            .Callback(() =>
            {
                Assert.Equal(parameter1, component.Property1);
                Assert.Equal(parameter2, component.Property2);
            });

        await rendererFake.Dispatcher.InvokeAsync(async () =>
            await component.SetParametersAsync(parameters));

        // act
        parameter1 = new Signal<int>(0);
        parameters = ParameterView.FromDictionary(new Dictionary<string, object?>()
        {
            { nameof(TestComponent.Property1), parameter1 },
            { nameof(TestComponent.Property2), parameter2 }
        });

        await rendererFake.Dispatcher.InvokeAsync(async () =>
            await component.SetParametersAsync(parameters));

        // assert
        Assert.Equal(parameter1, component.Property1);
        Assert.Equal(parameter2, component.Property2);

        buildRenderTree.Verify(obj => obj.Invoke(), Times.Exactly(2));
    }
}
