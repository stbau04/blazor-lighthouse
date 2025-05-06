using BlazorLighthouse.Internal;
using BlazorLighthouse.Internal.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Diagnostics;

namespace BlazorLighthouse.Core;

/// <summary>
/// Base class for components that autmatically tracks <see cref="ReadonlySignal{T}"/> accessed while rendering.
/// </summary>
public class LighthouseComponentBase 
    : SignalingContext, IComponent, IRefreshable, IHandleEvent, IHandleAfterRender
{
    private readonly RenderFragment renderFragment;
    private readonly AccessTracker accessTracker;
    private readonly Lock lockObject = new();

    private RenderHandle renderHandle;
    private (IComponentRenderMode? mode, bool cached) renderMode;

    private long renderId;
    private bool initialized;
    private bool hasNeverRendered = true;
    private bool hasCalledOnAfterRender;

    internal bool HasPendingQueuedRender { get; private set; }

    /// <summary>
    /// Gets the <see cref="Microsoft.AspNetCore.Components.RendererInfo"/> the component is running on.
    /// </summary>
    protected RendererInfo RendererInfo => renderHandle.RendererInfo;

    /// <summary>
    /// Gets the <see cref="ResourceAssetCollection"/> for the application.
    /// </summary>
    protected ResourceAssetCollection Assets => renderHandle.Assets;

    /// <summary>
    /// Gets the <see cref="IComponentRenderMode"/> assigned to this component.
    /// </summary>
    protected IComponentRenderMode? AssignedRenderMode
    {
        get
        {
            if (!renderMode.cached)
                renderMode = (renderHandle.RenderMode, true);

            return renderMode.mode;
        }
    }

    /// <summary>
    /// Constructs an instance of <see cref="LighthouseComponentBase"/>.
    /// </summary>
    public LighthouseComponentBase()
    {
        renderFragment = TrackAndBuildRenderTree;
        accessTracker = new(this, this);
    }

    /// <summary>
    /// Attach component to a render handle
    /// </summary>
    /// <param name="renderHandle">Render handle to attach component to</param>
    public void Attach(RenderHandle renderHandle)
    {
        if (this.renderHandle.IsInitialized)
            throw new InvalidOperationException($"Render handle is already set");

        this.renderHandle = renderHandle;
    }

    /// <summary>
    /// Sets parameters supplied by the component's parent in the render tree.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    /// <returns>A <see cref="Task"/> that completes when the component has finished updating and rendering itself.</returns>
    /// <remarks>
    /// <para>
    /// Parameters are passed when <see cref="SetParametersAsync(ParameterView)"/> is called. It is not required that
    /// the caller supply a parameter value for all of the parameters that are logically understood by the component.
    /// </para>
    /// <para>
    /// The default implementation of <see cref="SetParametersAsync(ParameterView)"/> will set the value of each property
    /// decorated with <see cref="ParameterAttribute" /> or <see cref="CascadingParameterAttribute" /> that has
    /// a corresponding value in the <see cref="ParameterView" />. Parameters that do not have a corresponding value
    /// will be unchanged.
    /// </para>
    /// </remarks>
    public Task SetParametersAsync(ParameterView parameters)
    {
        parameters.SetParameterProperties(this);
        var callStateHasChanged = EnforceStateHasChanged();

        if (!initialized)
        {
            initialized = true;
            return CallInitAndSetParametersAsync(callStateHasChanged);
        }

        return CallOnParametersSetAsync(callStateHasChanged);
    }

    /// <summary>
    /// Notifies the component that its state has changed. When applicable, this will
    /// cause the component to be re-rendered.
    /// </summary>
    protected void StateHasChanged()
    {
        QueueRenderingIfNecessary(renderId);
    }

    /// <summary>
    /// Executes the supplied work item on the associated renderer's
    /// synchronization context.
    /// </summary>
    /// <param name="workItem">The work item to execute.</param>
    protected Task InvokeAsync(Action workItem)
    {
        return renderHandle.Dispatcher.InvokeAsync(workItem);
    }

    /// <summary>
    /// Executes the supplied work item on the associated renderer's
    /// synchronization context.
    /// </summary>
    /// <param name="workItem">The work item to execute.</param>
    protected Task InvokeAsync(Func<Task> workItem)
    {
        return renderHandle.Dispatcher.InvokeAsync(workItem);
    }

    /// <summary>
    /// Treats the supplied <paramref name="exception"/> as being thrown by this component. This will cause the
    /// enclosing ErrorBoundary to transition into a failed state. If there is no enclosing ErrorBoundary,
    /// it will be regarded as an exception from the enclosing renderer.
    ///
    /// This is useful if an exception occurs outside the component lifecycle methods, but you wish to treat it
    /// the same as an exception from a component lifecycle method.
    /// </summary>
    /// <param name="exception">The <see cref="Exception"/> that will be dispatched to the renderer.</param>
    /// <returns>A <see cref="Task"/> that will be completed when the exception has finished dispatching.</returns>
    protected Task DispatchExceptionAsync(Exception exception)
    {
        return renderHandle.DispatchExceptionAsync(exception);
    }

    /// <summary>
    /// Renders the component to the supplied <see cref="RenderTreeBuilder"/>.
    /// </summary>
    /// <param name="builder">A <see cref="RenderTreeBuilder"/> that will receive the render output.</param>
    protected virtual void BuildRenderTree(RenderTreeBuilder builder)
    {

    }

    /// <summary>
    /// Method invoked when the component is ready to start, having received its
    /// initial parameters from its parent in the render tree.
    /// </summary>
    protected virtual void OnInitialized()
    {

    }

    /// <summary>
    /// Method invoked when the component is ready to start, having received its
    /// initial parameters from its parent in the render tree.
    ///
    /// Override this method if you will perform an asynchronous operation and
    /// want the component to refresh when that operation is completed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    protected virtual Task OnInitializedAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Method invoked when the component has received parameters from its parent in
    /// the render tree, and the incoming values have been assigned to properties.
    /// </summary>
    protected virtual void OnParametersSet()
    {

    }

    /// <summary>
    /// Method invoked when the component has received parameters from its parent in
    /// the render tree, and the incoming values have been assigned to properties.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    protected virtual Task OnParametersSetAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Method invoked after each time the component has rendered interactively and the UI has finished
    /// updating (for example, after elements have been added to the browser DOM). Any <see cref="ElementReference" />
    /// fields will be populated by the time this runs.
    ///
    /// This method is not invoked during prerendering or server-side rendering, because those processes
    /// are not attached to any live browser DOM and are already complete before the DOM is updated.
    /// </summary>
    /// <param name="firstRender">
    /// Set to <c>true</c> if this is the first time <see cref="OnAfterRender(bool)"/> has been invoked
    /// on this component instance; otherwise <c>false</c>.
    /// </param>
    /// <remarks>
    /// The <see cref="OnAfterRender(bool)"/> and <see cref="OnAfterRenderAsync(bool)"/> lifecycle methods
    /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
    /// Use the <paramref name="firstRender"/> parameter to ensure that initialization work is only performed
    /// once.
    /// </remarks>
    protected virtual void OnAfterRender(bool firstRender)
    {

    }

    /// <summary>
    /// Method invoked after each time the component has been rendered interactively and the UI has finished
    /// updating (for example, after elements have been added to the browser DOM). Any <see cref="ElementReference" />
    /// fields will be populated by the time this runs.
    ///
    /// This method is not invoked during prerendering or server-side rendering, because those processes
    /// are not attached to any live browser DOM and are already complete before the DOM is updated.
    ///
    /// Note that the component does not automatically re-render after the completion of any returned <see cref="Task"/>,
    /// because that would cause an infinite render loop.
    /// </summary>
    /// <param name="firstRender">
    /// Set to <c>true</c> if this is the first time <see cref="OnAfterRender(bool)"/> has been invoked
    /// on this component instance; otherwise <c>false</c>.
    /// </param>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    /// <remarks>
    /// The <see cref="OnAfterRender(bool)"/> and <see cref="OnAfterRenderAsync(bool)"/> lifecycle methods
    /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
    /// Use the <paramref name="firstRender"/> parameter to ensure that initialization work is only performed
    /// once.
    /// </remarks>
    protected virtual Task OnAfterRenderAsync(bool firstRender)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns a flag to indicate whether the component should render.
    /// </summary>
    /// <returns></returns>
    protected virtual bool ShouldRender()
    {
        return true;
    }

    /// <summary>
    /// Returns a flag to indicate when state has changed should be called.
    /// </summary>
    /// <returns></returns>
    protected virtual bool EnforceStateHasChanged() {
        return true;
    }

    private void TrackAndBuildRenderTree(RenderTreeBuilder builder)
    {
        accessTracker.Track(() =>
        {
            renderId++;
            HasPendingQueuedRender = false;
            hasNeverRendered = false;
            BuildRenderTree(builder);
        });
    }

    private async Task CallInitAndSetParametersAsync(bool parameters)
    {
        var onInitTask = CallOnInitializedAsync();
        if (!ShouldAwaitTask(onInitTask))
        {
            await CallOnParametersSetAsync(parameters);
            return;
        }

        if (parameters)
            StateHasChanged();

        await WaitForAsyncTaskCompletion(onInitTask);
        await CallOnParametersSetAsync(parameters);
    }

    [DebuggerDisableUserUnhandledExceptions]
    private Task CallOnInitializedAsync()
    {
        try
        {
            OnInitialized();
            return OnInitializedAsync();
        }
        catch (Exception ex) when (ex is not NavigationException)
        {
            Debugger.BreakForUserUnhandledException(ex);
            throw;
        }
    }

    private static bool ShouldAwaitTask(Task task)
    {
        return task.Status is not TaskStatus.RanToCompletion
            and not TaskStatus.Canceled;
    }

    private Task CallOnParametersSetAsync(bool callStateHasChanged)
    {
        var onParametersSetTask = CallOnParametersSetAsync();
        return CallStateHasChangedAfterAsyncTask(
            onParametersSetTask,
            callStateHasChanged);
    }

    [DebuggerDisableUserUnhandledExceptions]
    private Task CallOnParametersSetAsync()
    {
        try
        {
            OnParametersSet();
            return OnParametersSetAsync();
        }
        catch (Exception ex) when (ex is not NavigationException)
        {
            Debugger.BreakForUserUnhandledException(ex);
            throw;
        }
    }

    private Task CallStateHasChangedAfterAsyncTask(Task task, bool callStateHasChanged)
    {
        var shouldAwaitTask = ShouldAwaitTask(task);

        if (callStateHasChanged)
            StateHasChanged();

        if (!shouldAwaitTask)
            return Task.CompletedTask;

        return CallStateHasChangedOnAsyncCompletion(task, callStateHasChanged);
    }

    private async Task CallStateHasChangedOnAsyncCompletion(Task task, bool callStateHasChanged)
    {
        if (!await WaitForAsyncTaskCompletion(task) 
            || !callStateHasChanged)
        {
            return;
        }

        StateHasChanged();
    }

    [DebuggerDisableUserUnhandledExceptions]
    private static async Task<bool> WaitForAsyncTaskCompletion(Task task)
    {
        try
        {
            await task;
        }
        catch
        {
            if (task.IsCanceled)
                return false;

            throw;
        }

        return true;
    }

    private void QueueRendering()
    {
        try
        {
            var canQueueRendering = CanQueueRendering();
            if (!canQueueRendering)
                return;

            renderHandle.Render(renderFragment);
        }
        catch
        {
            HasPendingQueuedRender = false;
            throw;
        }
    }

    private bool CanQueueRendering()
    {
        lock (lockObject)
        {
            if (HasPendingQueuedRender)
                return false;

            HasPendingQueuedRender = true;
            return true;
        }
    }

    private void QueueRenderingIfNecessary(long previousRenderId)
    {
        if (HasPendingQueuedRender || previousRenderId != renderId)
            return;

        if (!hasNeverRendered
            && !ShouldRender()
            && !renderHandle.IsRenderingOnMetadataUpdate)
        {
            return;
        }

        QueueRendering();
    }

    void IRefreshable.Refresh()
    {
        if (HasPendingQueuedRender)
            return;

        var renderId = this.renderId;
        InvokeAsync(() => QueueRenderingIfNecessary(renderId));
    }

    void IRefreshable.Dispose(AbstractSignal signal)
    {
        accessTracker.Untrack(signal);
    }

    Task IHandleEvent.HandleEventAsync(EventCallbackWorkItem callback, object? arg)
    {
        var eventTask = callback.InvokeAsync(arg);
        var callStateHasChanged = EnforceStateHasChanged();
        return CallStateHasChangedAfterAsyncTask(eventTask, callStateHasChanged);
    }

    Task IHandleAfterRender.OnAfterRenderAsync()
    {
        var firstRender = !hasCalledOnAfterRender;
        hasCalledOnAfterRender = true;

        OnAfterRender(firstRender);
        return OnAfterRenderAsync(firstRender);
    }
}
