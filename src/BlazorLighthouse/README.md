> This project is undergoing major restructuring before the first official release. The currently available versions are only experimental and mainly used for detailed testing. Do not use it for real projects in any case.

# Blazor Lighthouse
*Blazor Lighthouse* provides an API for change detection, therefore allowing recalculations only when it is actually necessary. The concept is pretty much the same as the signals that are available in [Angular](https://angular.dev/guide/signals).

- **How does this work?** *Blazor Lighthouse* provides a wrapper for values it should be used on
- **What are the restrictions?** The usage is currently restricted to synchronous tasks
- **What are the disadvantages?** There are two main disadvantages: Accessing a variable takes more computation time (as they are performing the change detection) and it results in slightly less maintainable code by default

## About the project
Currently, the project is only a proof of concept. Mainly the Blazor integration is not fully implemented yet, there are some heavy optimizations to be made. If there are any ideas/feedback, it is always welcome.
