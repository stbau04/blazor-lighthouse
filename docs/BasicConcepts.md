# Basic Concepts of Blazor Lighthouse
**Signals** and **Effects** are the main building blocks of *Blazor Lighthouse*. **Signals** provide the value stores which can later be used in **Computed Values** and **Effects**.

## Signals
**Signals** are simple value stores. They require an initial value, which can be changed later. Any access inside of an **Effect, Computed Value or Component** leads to an subscription (this behavior can not be nested).

```
 // Create signal
var signal = new Signal<int>(0);

// Set signal value
signal.Set(1);

// Access signal value
int value = signal.Get();
```

## Effects
**Effects** are simple subscribers. They accept a callback performing an arbitrary action. Initially, the action is run when the the effect is initialized. Afterwards whenever an accessed **Signal** value changes the **Effect** is rerun.

```
// Create effect and run callback
_ = new Effect(() => {
    Console.WriteLine($"The value is: {signal.Get()}");
});

// Runs callback again
signal.Set(1);
```

## Computed Values
**Computed Values** combine the fuctionality of **Signals** with the functionality of an **Effect**. It allows the calculation of a value as it would be done with an **Effect** and provides this value as a readonly **Signal**.

```
// Create computed and calculate value
var computed = new Computed<int>(() => {
    return signal.Get() * signal.Get();
});

// Calculates value agian
signal.Set(1);

// Access computed value
int value = computed.Get();
```

<br/>
<p align="center">
    <img src="../img/logo.svg" width="200px" alt="Logo">
</p>