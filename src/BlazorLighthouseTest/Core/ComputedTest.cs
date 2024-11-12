using BlazorLighthouse.Core;

namespace BlazorLighthouseTest.Core;

public class ComputedTest
{
    [Fact]
    public void TestGet()
    {
        // arrange
        var recalculationCount = 0;

        var signal = new Signal<int>(1);

        var computed = new Computed<int>(() =>
        {
            recalculationCount++;
            return signal.Get();
        });

        // act
        var value = computed.Get();

        // assert
        Assert.Equal(1, value);
        Assert.Equal(1, recalculationCount);
    }

    [Fact]
    public void TestValueAccessedMultipleTimes()
    {
        // arrange
        var recalculationCount = 0;

        var signal = new Signal<int>(1);

        var computed = new Computed<int>(() =>
        {
            recalculationCount++;
            return signal.Get();
        });

        // act
        var value1 = computed.Get();
        var value2 = computed.Get();

        // assert
        Assert.Equal(1, value1);
        Assert.Equal(1, value2);
        Assert.Equal(1, recalculationCount);
    }

    [Fact]
    public void TestValueChanged()
    {
        // arrange
        var recalculationCount = 0;

        var signal = new Signal<int>(1);

        var computed = new Computed<int>(() =>
        {
            recalculationCount++;
            return signal.Get();
        });

        // act
        signal.Set(2);

        // assert
        Assert.Equal(2, computed.Get());
        Assert.Equal(2, recalculationCount);
    }

    [Fact]
    public void TestValueNotChanged()
    {
        // arrange
        var recalculationCount = 0;

        var signal = new Signal<int>(1);

        var computed = new Computed<int>(() =>
        {
            recalculationCount++;
            return signal.Get();
        });

        // act
        signal.Set(1);

        // assert
        Assert.Equal(1, computed.Get());
        Assert.Equal(1, recalculationCount);
    }

    [Fact]
    public void TestValueChangedMultipleTimes()
    {
        // arrange
        var recalculationCount = 0;

        var signal = new Signal<int>(1);

        var computed = new Computed<int>(() =>
        {
            recalculationCount++;
            return signal.Get();
        });

        // act
        signal.Set(2);
        signal.Set(3);

        // assert
        Assert.Equal(3, computed.Get());
        Assert.Equal(3, recalculationCount);
    }

    [Fact]
    public void TestUnreferencedValueChanged()
    {
        // arrange
        var recalculationCount = 0;

        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);

        var computed = new Computed<int>(() =>
        {
            recalculationCount++;
            return signal1.Get();
        });

        // act
        signal2.Set(3);

        // assert
        Assert.Equal(1, computed.Get());
        Assert.Equal(1, recalculationCount);
    }

    [Fact]
    public void TestUnreferencedValueChanged_WasReferenced()
    {
        // arrange
        var recalculationCount = 0;

        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);

        var computed = new Computed<int>(() =>
        {
            recalculationCount++;
            if (signal1.Get() == 3)
                return signal1.Get();
            return signal2.Get();
        });

        // act
        signal1.Set(3);
        signal2.Set(4);

        // assert
        Assert.Equal(3, computed.Get());
        Assert.Equal(2, recalculationCount);
    }

    [Fact]
    public void TestNestedComputedValue()
    {
        // arrange
        var recalculationCount1 = 0;
        var recalculationCount2 = 0;

        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);

        var computed1 = new Computed<int>(() =>
        {
            recalculationCount1++;
            return signal1.Get();
        });

        var computed2 = new Computed<int>(() =>
        {
            recalculationCount2++;
            return computed1.Get() + signal2.Get();
        });

        // act
        var value = computed2.Get();

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

        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);

        var computed1 = new Computed<int>(() =>
        {
            recalculationCount1++;
            return signal1.Get();
        });

        var computed2 = new Computed<int>(() =>
        {
            recalculationCount2++;
            return computed1.Get() + signal2.Get();
        });

        // act
        signal2.Set(3);

        // assert
        Assert.Equal(4, computed2.Get());
        Assert.Equal(1, recalculationCount1);
        Assert.Equal(2, recalculationCount2);
    }

    [Fact]
    public void TestNestedComputedValue_NestedValueChanged()
    {
        // arrange
        var recalculationCount1 = 0;
        var recalculationCount2 = 0;

        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);

        var computed1 = new Computed<int>(() =>
        {
            recalculationCount1++;
            return signal1.Get();
        });

        var computed2 = new Computed<int>(() =>
        {
            recalculationCount2++;
            return computed1.Get() + signal2.Get();
        });

        // act
        signal1.Set(3);

        // assert
        Assert.Equal(5, computed2.Get());
        Assert.Equal(2, recalculationCount1);
        Assert.Equal(2, recalculationCount2);
    }
}
