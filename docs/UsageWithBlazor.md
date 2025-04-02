# Usage with Blazor Components
The **LighthouseComponentBase** provides a Blazor component base for the signaling technology. All signal values that are accessed during rendering (specified inside the razor template or any code that is synchronously run while rendering) leads to a subscription and triggers a re-rendering if changed. Further the component base inherits from the **SignalingContext** allowing values to diretly match the components lifetime.

# Using LighthouseComponentBase in a Blazor Component
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

# How It Works
- LighthouseComponentBase inherits from SignalingContext, ensuring proper disposal of signals.
- When Value.Set() is called, the component automatically re-renders.
- No need for StateHasChanged()—Lighthouse detects the update.

# Key Benefits of Using LighthouseComponentBase
- Automatic Subscription Tracking – Any accessed signal inside @code or the Razor template automatically triggers reactivity.
- Efficient Updates – Only re-renders when needed, reducing unnecessary UI updates.
- Memory Safety – The component manages its own signals, preventing memory leaks.

<br/>
<p align="center">
    <img src="../img/logo.svg" width="200px" alt="Logo">
</p>