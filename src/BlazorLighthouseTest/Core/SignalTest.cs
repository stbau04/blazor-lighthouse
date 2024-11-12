using BlazorLighthouse.Core;

namespace BlazorLighthouseTest.Core;

public class SignalTest
{
    [Fact]
    public void TestGet()
    {
        // arrange
        var signal = new Signal<int>(1);

        // assert
        Assert.Equal(1, signal.Get());
    }

    [Fact]
    public void TestSet()
    {
        // arrange
        var signal = new Signal<int>(1);

        // act
        signal.Set(2);

        // assert
        Assert.Equal(2, signal.Get());
    }
    

    [Fact]
    public void TestSet_ValueNotChanged()
    {
        // arrange
        var signal = new Signal<int>(1);

        // act
        signal.Set(1);

        // assert
        Assert.Equal(1, signal.Get());
    }

    [Fact]
    public void TestSet_ValueChangedMultipleTimes()
    {
        // arrange
        var signal = new Signal<int>(1);

        // act
        signal.Set(2);
        signal.Set(3);

        // assert
        Assert.Equal(3, signal.Get());
    }
}
