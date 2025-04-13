using BlazorLighthouse.Core;

namespace BlazorLighthouseTest.Core;

public class EffectTest
{
    [Fact]
    public void TestEffect()
    {
        // arrange
        var recalculationCount = 0;
        var value = 0;

        var signal = new Signal<int>(1);

        _ = new Effect(() =>
        {
            recalculationCount++;
            value = signal.Get();
        });

        // assert
        Assert.Equal(1, value);
        Assert.Equal(1, recalculationCount);
    }
    
    [Fact]
    public void TestValueChanged()
    {
        // arrange
        var recalculationCount = 0;
        var value = 0;

        var signal = new Signal<int>(1);

        _ = new Effect(() =>
        {
            recalculationCount++;
            value = signal.Get();
        });

        // act
        signal.Set(2);

        // assert
        Assert.Equal(2, value);
        Assert.Equal(2, recalculationCount);
    }

    [Fact]
    public void TestValueNotChanged()
    {
        // arrange
        var recalculationCount = 0;
        var value = 0;

        var signal = new Signal<int>(1);

        _ = new Effect(() =>
        {
            recalculationCount++;
            value = signal.Get();
        });

        // act
        signal.Set(1);

        // assert
        Assert.Equal(1, value);
        Assert.Equal(1, recalculationCount);
    }

    [Fact]
    public void TestValueChangedMultipleTimes()
    {
        // arrange
        var recalculationCount = 0;
        var value = 0;

        var signal = new Signal<int>(1);

        _ = new Effect(() =>
        {
            recalculationCount++;
            value = signal.Get();
        });

        // act
        signal.Set(2);
        signal.Set(3);

        // assert
        Assert.Equal(3, value);
        Assert.Equal(3, recalculationCount);
    }

    [Fact]
    public void TestUnreferencedValueChanged()
    {
        // arrange
        var recalculationCount = 0;
        var value = 0;

        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);

        _ = new Effect(() =>
        {
            recalculationCount++;
            value = signal1.Get();
        });

        // act
        signal2.Set(3);

        // assert
        Assert.Equal(1, value);
        Assert.Equal(1, recalculationCount);
    }

    [Fact]
    public void TestUnreferencedValueChanged_WasReferenced()
    {
        // arrange
        var recalculationCount = 0;
        var value = 0;

        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);

        _ = new Effect(() =>
        {
            recalculationCount++;
            if (signal1.Get() == 3)
            {
                value = signal1.Get();
                return;
            }

            value = signal2.Get();
        });

        // act
        signal1.Set(3);
        signal2.Set(4);

        // assert
        Assert.Equal(3, value);
        Assert.Equal(2, recalculationCount);
    }

    [Fact]
    public void TestNestedComputedValue()
    {
        // arrange
        var recalculationCount1 = 0;
        var recalculationCount2 = 0;
        var value = 0;

        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);

        var computed = new Computed<int>(() => {
            recalculationCount1++;
            return signal1.Get();
        });

        _ = new Effect(() =>
        {
            recalculationCount2++;
            value = computed.Get() + signal2.Get();
        });

        // assert
        Assert.Equal(3, value);
        Assert.Equal(1, recalculationCount1);
        Assert.Equal(1, recalculationCount2);
    }

    [Fact]
    public void TestNestedComputedValue_ValueChanged()
    {
        // arrange
        var recalculationCount1 = 0;
        var recalculationCount2 = 0;
        var value = 0;

        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);

        var computed = new Computed<int>(() => {
            recalculationCount1++;
            return signal1.Get();
        });

        _ = new Effect(() =>
        {
            recalculationCount2++;
            value = computed.Get() + signal2.Get();
        });

        // act
        signal2.Set(3);

        // assert
        Assert.Equal(4, value);
        Assert.Equal(1, recalculationCount1);
        Assert.Equal(2, recalculationCount2);
    }

    [Fact]
    public void TestNestedComputedValue_NestedValueChanged()
    {
        // arrange
        var recalculationCount1 = 0;
        var recalculationCount2 = 0;
        var value = 0;

        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);

        var computed = new Computed<int>(() => {
            recalculationCount1++;
            return signal1.Get();
        });

        _ = new Effect(() =>
        {
            recalculationCount2++;
            value = computed.Get() + signal2.Get();
        });

        // act
        signal1.Set(3);

        // assert
        Assert.Equal(5, value);
        Assert.Equal(2, recalculationCount1);
        Assert.Equal(2, recalculationCount2);
    }

    [Fact]
    public async Task TestMultipleSignalChanges()
    {
        // arrange
        var recalculationCount = 0;
        var value = 0;

        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);
        var signal3 = new Signal<int>(2);

        var taskCompletionSource1 = new TaskCompletionSource();
        var taskCompletionSource2 = new TaskCompletionSource();

        taskCompletionSource1.SetResult();
        var effect = new Effect(() =>
        {
            signal1.Get();
            signal2.Get();
            signal3.Get();

            taskCompletionSource2.SetResult();
            taskCompletionSource1.Task.Wait();

            recalculationCount++;
            value = signal3.Get();
        });

        // act
        taskCompletionSource1 = new();
        taskCompletionSource2 = new();

        var setterTask1 = Task.Run(() => signal1.Set(2));
        await taskCompletionSource2.Task;

        var setterTask2 = Task.Run(() => signal2.Set(3));
        while (!effect!.IsRunQueued)
            ;

        signal3.Set(4);

        taskCompletionSource2 = new();
        taskCompletionSource1.SetResult();

        await setterTask1;
        await setterTask2;

        // assert
        Assert.Equal(4, value);
        Assert.Equal(3, recalculationCount);
    }
}
