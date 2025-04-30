# Pitfalls of Memory Leaks
There are some situations where an **Effect, Computed Value or Component** subscribes to an value that has a longer lifespan as itself (or vice versa). For example when the signal value is provided through some signleton service - or just a simple static value.

These situations would lead to a memory leak: *Blazor Lighthouse* has to reference an element to know that something should be rerun. But does not know whether that element is actually still used anywhere. Therefor the **SignalingContext** was introduced. It provides an easy way of clearly defining the lifespan of an element.

## How BlazorLighthouse Solves This with SignalingContext
The SignalingContext acts as a scope for signals, ensuring that when a component or a set of signals is disposed of, all associated references are also released.

The signaling context is a disposable resouce that can be passed to a **Signal, Effect or Computed Value**. The **LighthouseComponentBase** inherits from the **SignalingContext** itself. As soon as the context gets disposed all values that are referencing that context get disposed instantly and can no longer be used in any way (performing an invalid action would lead to an exception).

## Managing Lifespan with SignalingContext
When defining Signals, Effects, or Computed values, attaching them to a SignalingContext can help manage their lifespan efficiently. This approach ensures that these reactive primitives are properly disposed of when the context (e.g., a Blazor component) is destroyed.

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
Once context.Dispose() is called, all elements referencing this context are automatically cleaned up.

<br/>
<p align="center">
    <img src="../img/logo.svg" width="200px" alt="Logo">
</p>