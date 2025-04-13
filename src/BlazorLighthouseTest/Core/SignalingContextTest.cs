using BlazorLighthouse.Core;

namespace BlazorLighthouseTest.Core;

public class SignalingContextTest
{
    [Fact]
    public void TestContextDisposed()
    {
        // arrange
        var signalingContext = new SignalingContext();
        
        // act
        signalingContext.Dispose();

        // assert
        Assert.Throws<InvalidOperationException>(
            () => new Signal<int>(signalingContext, 0));
        Assert.Throws<InvalidOperationException>(
            () => new Computed<int>(signalingContext, () => 0));
        Assert.Throws<InvalidOperationException>(
            () => new Effect(signalingContext, () => { }));
    }

    [Fact]
    public void TestSignal()
    {
        // arrange
        var signalingContext = new SignalingContext();
        var signal = new Signal<int>(signalingContext, 1);

        // act
        signal.Set(2);

        // assert
        Assert.Equal(2, signal.Get());
    }

    [Fact]
    public void TestSignal_ContextDisposed()
    {
        // arrange
        var signalingContext = new SignalingContext();
        var signal = new Signal<int>(signalingContext, 1);

        // act
        signalingContext.Dispose();

        // assert
        Assert.Throws<InvalidOperationException>(
            () => signal.Get());
        Assert.Throws<InvalidOperationException>(
            () => signal.Set(2));
    }

    [Fact]
    public void TestSignalInUseByComputed_ContextDisposed()
    {
        // arrange
        var recalculationCount = 0;

        var signalingContext = new SignalingContext();
        var signal = new Signal<int>(signalingContext, 1);
        var computed = new Computed<int>(() =>
        {
            recalculationCount++;
            return signal.Get();
        });

        // act
        signalingContext.Dispose();

        // assert
        Assert.Throws<InvalidOperationException>(
            () => signal.Set(2));

        Assert.Equal(1, computed.Get());
        Assert.Equal(1, recalculationCount);
    }
    
    [Fact]
    public void TestSignalInUseByEffect_ContextDisposed()
    {
        // arrange
        var recalculationCount = 0;
        var value = 0;

        var signalingContext = new SignalingContext();
        var signal = new Signal<int>(signalingContext, 1);
        _ = new Effect(() =>
        {
            recalculationCount++;
            value = signal.Get();
        });

        // act
        signalingContext.Dispose();

        // assert
        Assert.Throws<InvalidOperationException>(
            () => signal.Set(2));

        Assert.Equal(1, value);
        Assert.Equal(1, recalculationCount);
    }

    [Fact]
    public void TestComputed()
    {
        // arrange
        var recalculationCount = 0;

        var signalingContext = new SignalingContext();
        var signal = new Signal<int>(1);
        var computed = new Computed<int>(signalingContext, () =>
        {
            recalculationCount++;
            return signal.Get();
        });

        // act
        signal.Set(2);

        // assert
        Assert.Equal(2, recalculationCount);
        Assert.Equal(2, computed.Get());
    }
   

    [Fact]
    public void TestComputed_ContextDisposed()
    {
        // arrange
        var recalculationCount = 0;

        var signalingContext = new SignalingContext();
        var signal = new Signal<int>(1);
        var computed = new Computed<int>(signalingContext, () =>
        {
            recalculationCount++;
            return signal.Get();
        });

        // act
        signalingContext.Dispose();
        signal.Set(2);

        // assert
        Assert.Equal(1, recalculationCount);
        Assert.Throws<InvalidOperationException>(
            () => computed.Get());
    }
    
    [Fact]
    public void TestComputedInUseByComputed_ContextDisposed()
    {
        // arrange
        var recalculationCount = 0;

        var signalingContext = new SignalingContext();
        var signal = new Signal<int>(1);
        var computed1 = new Computed<int>(signalingContext, signal.Get);
        var computed2 = new Computed<int>(() =>
        {
            recalculationCount++;
            return computed1.Get();
        });

        // act
        signalingContext.Dispose();
        signal.Set(2);

        // assert
        Assert.Equal(1, recalculationCount);
        Assert.Equal(1, computed2.Get());
    }
    
    [Fact]
    public void TestComputedInUseByEffect_ContextDisposed()
    {
        // arrange
        var recalculationCount = 0;
        var value = 0;

        var signalingContext = new SignalingContext();
        var signal = new Signal<int>(1);
        var computed = new Computed<int>(signalingContext, signal.Get);
        var effect = new Effect(signalingContext, () =>
        {
            recalculationCount++;
            value = computed.Get();
        });

        // act
        signalingContext.Dispose();
        signal.Set(2);

        // assert
        Assert.Equal(1, value);
        Assert.Equal(1, recalculationCount);
    }

    [Fact]
    public void TestEffect()
    {
        // arrange
        var recalculationCount = 0;
        var value = 0;

        var signalingContext = new SignalingContext();
        var signal = new Signal<int>(1);
        var effect = new Effect(signalingContext, () =>
        {
            recalculationCount++;
            value = signal.Get();
        });

        // act
        signal.Set(2);

        // assert
        Assert.Equal(2, recalculationCount);
        Assert.Equal(2, value);
    }

    [Fact]
    public void TestEffect_ContextDisposed()
    {
        // arrange
        var recalculationCount = 0;
        var value = 0;

        var signalingContext = new SignalingContext();
        var signal = new Signal<int>(1);
        var effect = new Effect(signalingContext , () =>
        {
            recalculationCount++;
            value = signal.Get();
        });

        // act
        signalingContext.Dispose();
        signal.Set(2);

        // assert
        Assert.Equal(1, recalculationCount);
        Assert.Equal(1, value);
    }
}
