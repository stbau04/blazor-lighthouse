# Usage with Blazor Components
The **LighthouseComponentBase** provides a Blazor component base for the signaling technology. All signal values that are accessed during rendering (specified inside the razor template or any code that is synchronously run while rendering) leads to a subscription and triggers a re-rendering if changed. Further the component base inherits from the **SignalingContext** allowing values to diretly match the components lifetime.

## Using LighthouseComponentBase in a Blazor Component
To leverage automatic updates in a Blazor component, inherit from LighthouseComponentBase:

```
@inherits LighthouseComponentBase

<h3>Current Value: @Value.Get()</h3>
<button @onclick="Increment">Increment</button>

@code {
    public Signal<int> Value { get; }

    public MyComponent()
    {
        Value = new(this, 0);
    }

    private void Increment()
    {
        Value.Set(Value.Get() + 1);
    }
}
```

## How It Works
- LighthouseComponentBase inherits from SignalingContext, providing a mechanism to bind the lifespan of a Signal to the lifespan of a Component, helping manage the disposal of signals.
- When Value.Set() is called, the component automatically re-renders if the signal's value is used during rendering. This ensures that only components dependent on the signal are updated.
- No need for StateHasChanged() - Lighthouse detects the update.


## Key Benefits of Using LighthouseComponentBase
- Automatic Subscription Tracking – Any signal accessed inside the Razor template automatically triggers reactivity and re-rendering. Signals accessed in the @code block will not trigger reactivity.
- Efficient Updates – Only re-renders when needed, reducing unnecessary UI updates.
- Memory Safety – The developer must explicitly use the component as the context parameter when creating signals (e.g., new Signal(this, 0)) to ensure proper signal disposal and prevent memory leaks.

<br/>
<p align="center">
    <img src="../img/logo.svg" width="200px" alt="Logo">
</p>