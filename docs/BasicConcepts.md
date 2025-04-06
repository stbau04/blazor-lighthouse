# Basic Concepts of Blazor Lighthouse
Blazor Lighthouse is a lightweight, high-performance state management library for Blazor applications that implements the signals pattern. While the term "signals pattern" may not be a widely recognized design pattern, it is conceptually similar to reactive programming and observable patterns. These paradigms are commonly used in frameworks like ReactiveX or SolidJS, where reactive primitives (like signals) are used to manage state and reactivity.

For more information on reactive programming concepts, pleaser refer to:
- [Reactive Programming Overview](https://en.wikipedia.org/wiki/Reactive_programming)
- [Signals in SolidJS](https://www.solidjs.com/docs/latest#signals)

**Signals** and **Effects** are the main building blocks of *Blazor Lighthouse*. **Signals** provide the value stores which can later be used in **Computed Values** and **Effects**.

## Signals
**Signals** are simple value stores. They require an initial value, which can be changed later. Any access inside of an **Effect, Computed Value or Component** leads to an subscription (this behavior can not be nested).

Signals are the foundation of Blazor Lighthouse's reactivity system. They:
- Store mutable state
- Automatically notify their dependents (e.g., components or computations) when their value changes

## What Are Dependents?
In this context, dependents are the components, UI elements, or reactive computations that rely on the value of a signal. When a signal's value changes, all its dependents are automatically updated to reflect the new state. This ensures that the UI stays in sync with the underlying data without requiring manual updates.

```
 // Create signal
Signal<int> signal = new Signal<int>(0);

// Set signal value
signal.Set(1);

// Access signal value
int value = signal.Get();
```

### Key Features:
- Type-safe value storage
- Change detection (won't notify if value hasn't changed)
- Batch updates (multiple sets only trigger one notification)
- Context-aware lifetime management

## Effects
**Effects** are simple subscribers. They accept a callback performing an arbitrary action. Initially, the action is run when the the effect is initialized. Afterwards whenever an accessed **Signal** value changes the **Effect** is rerun.

### Common Use Cases:
- Console logging
- DOM manipulation
- API calls
- State synchronization

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

### Characteristics:
- Lazy evaluation (only compute when needed)
- Memoization (cache results until dependencies change)
- Automatic dependency tracking
- Read-only interface

```
// Create computed and calculate value
Computed<int> computed = new Computed<int>(() => {
    return signal.Get() * signal.Get();
});

// Calculates value agian
signal.Set(1);

// Access computed value
int value = computed.Get();
```

## When to Use Blazor Lighthouse
- Your application has complex state relationships
- You need fine-grained reactivity for performance
- You want to minimize unnecessary re-renders
- You need to share state across components

## How to Use Blazor Lighthouse in Blazor Components?
Go to UsageWithBlazor.md

<br/>
<p align="center">
    <img src="../img/logo.svg" width="200px" alt="Logo">
</p>