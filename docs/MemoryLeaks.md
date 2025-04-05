# Pitfalls of Memory Leaks
There are some situations where an **Effect, Computed Value or Component** subscribes to an value that has a longer lifespan as itself (or vice versa). For example when the signal value is provided through some signleton service - or just a simple static value.

These situations would lead to a memory leak: *Blazor Lighthouse* has to reference an element to know that something should be rerun. But does not know whether that element is actually still used anywhere. Therefor the **SignalingContext** was introduced. It provides an easy way of clearly defining the lifespan of an element.

## The SignalingContext
The signaling context is a disposable resouce that can be passed to a **Signal, Effect or Computed Value**. The **LighthouseComponentBase** inherits from the **SignalingContext** itself. As soon as the context gets disposed all values that are referencing that context get disposed instantly and can no longer be used in any way (performing an invalid action would lead to an exception).

```
// Create signaling context
SignalingContext context = new SignalingContext();

// Create signal within context
Signal<int> signal = new Signal<int>(context, 0);

// Create effect within context
_ = new Effect(context, () => {
    Console.WriteLine($"The value is: {signal.Get()}");
});

// Create computed value within context
Computed<int> computed = new Computed<int>(context, () => {
    return signal.Get() * signal.Get();
}); 

// Dispose all elements created within this context
context.Dispose();
```

<br/>
<p align="center">
    <img src="../img/logo.svg" width="200px" alt="Logo">
</p>