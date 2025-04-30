using BlazorLighthouse.Internal;
using BlazorLighthouse.Internal.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Diagnostics;

namespace BlazorLighthouse.Core;

/// <summary>
/// Base for Blazor components that should subscribe to signal value changes 
/// </summary>
public class LighthouseComponentBase 
    : SignalingContext, IComponent, IRefreshable, IHandleAfterRender, IHandleEvent
{
    private readonly RenderFragment renderFragment;
    private readonly AccessTracker accessTracker;

    private RenderHandle renderHandle;
    private (IComponentRenderMode? mode, bool cached) renderMode;
    private IReadOnlyDictionary<string, object?>? parameters;

    private bool initialized;
    private bool hasNeverRendered = true;
    private bool hasPendingQueuedRender;
    private bool hasCalledOnAfterRender;

    /// <summary>
    /// 
    /// </summary>
    protected RendererInfo RendererInfo => renderHandle.RendererInfo;

    /// <summary>
    /// 
    /// </summary>
    protected ResourceAssetCollection Assets => renderHandle.Assets;

    /// <summary>
    /// 
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
    /// 
    /// </summary>
    protected bool PreserveDefaultRenderingBehavior { get; set; } = false;

    /// <summary>
    /// Instantiate a new lighthouse base component
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
        if (renderHandle.IsInitialized)
            throw new InvalidOperationException($"The render handle is already set. Cannot initialize a {nameof(ComponentBase)} more than once.");

        this.renderHandle = renderHandle;
    }

    /// <summary>
    /// Set the components paramters
    /// </summary>
    /// <param name="parameters">Parameter values</param>
    /// <returns>A completed task, as nothing is run async</returns>
    public Task SetParametersAsync(ParameterView parameters)
    {
        parameters.SetParameterProperties(this);
        var callStateHasChanged = PreserveDefaultRenderingBehavior 
            || HaveParamtersChanged(parameters);

        if (!initialized)
        {
            initialized = true;
            return RunInitAndSetParametersAsync(callStateHasChanged);
        }

        return CallOnParametersSetAsync(callStateHasChanged);
    }

    //TODO
    /// <summary>
    /// Re-render the component
    /// </summary>
    protected void StateHasChanged()
    {
        if (hasPendingQueuedRender)
            return;

        if (!hasNeverRendered 
            && !ShouldRender() 
            && !renderHandle.IsRenderingOnMetadataUpdate)
        {
            return;
        }

        try
        {
            hasPendingQueuedRender = true;
            renderHandle.Render(renderFragment);
        }
        catch
        {
            hasPendingQueuedRender = false;
            throw;
        }
    }

    /// <summary>
    /// Invoke action on the associated dispatcher
    /// </summary>
    /// <param name="workItem">Action to run</param>
    /// <returns>Task returned by dispatcher</returns>
    protected Task InvokeAsync(Action workItem)
    {
        return renderHandle.Dispatcher.InvokeAsync(workItem);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="workItem"></param>
    /// <returns></returns>
    protected Task InvokeAsync(Func<Task> workItem)
    {
        return renderHandle.Dispatcher.InvokeAsync(workItem);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    protected Task DispatchExceptionAsync(Exception exception)
    {
        return renderHandle.DispatchExceptionAsync(exception);
    }

    /// <summary>
    /// Build the render tree
    /// </summary>
    /// <param name="builder">Builder provided by the renderer</param>
    protected virtual void BuildRenderTree(RenderTreeBuilder builder)
    {

    }

    /// <summary>
    /// 
    /// </summary>
    protected virtual void OnInitialized()
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected virtual Task OnInitializedAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 
    /// </summary>
    protected virtual void OnParametersSet()
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected virtual Task OnParametersSetAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="firstRender"></param>
    protected virtual void OnAfterRender(bool firstRender)
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="firstRender"></param>
    /// <returns></returns>
    protected virtual Task OnAfterRenderAsync(bool firstRender)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected virtual bool ShouldRender()
    {
        return true;
    }

    private void TrackAndBuildRenderTree(RenderTreeBuilder builder)
    {
        accessTracker.Track(() =>
        {
            hasPendingQueuedRender = false;
            hasNeverRendered = false;
            BuildRenderTree(builder);
        });
    }

    private bool HaveParamtersChanged(ParameterView parameters)
    {
        var oldParamters = this.parameters;
        this.parameters = parameters.ToDictionary();
        if (oldParamters == null)
            return true;

        return this.parameters.Any(
            parameter => HasParameterChanged(oldParamters, parameter));
    }

    private bool HasParameterChanged(
        IReadOnlyDictionary<string, object?> oldParameters,
        KeyValuePair<string, object?> parameter)
    {
        if (parameter.Value is not AbstractSignal abstractSignal
            || !oldParameters.TryGetValue(parameter.Key, out var value))
        {
            return true;
        }

        return abstractSignal != value;
    }

    //TODO
    [DebuggerDisableUserUnhandledExceptions]
    private async Task RunInitAndSetParametersAsync(bool parameters)
    {
        Task task;

        try
        {
            OnInitialized();
            task = OnInitializedAsync();
        }
        catch (Exception ex) when (ex is not NavigationException)
        {
            Debugger.BreakForUserUnhandledException(ex);
            throw;
        }

        if (task.Status != TaskStatus.RanToCompletion && task.Status != TaskStatus.Canceled)
        {
            if (parameters)
                StateHasChanged();

            try
            {
                await task;
            }
            catch
            {
                if (!task.IsCanceled)
                {
                    throw;
                }
            }
        }

        await CallOnParametersSetAsync(parameters);
    }

    //TODO
    [DebuggerDisableUserUnhandledExceptions]
    private Task CallOnParametersSetAsync(bool parameters)
    {
        Task task;

        try
        {
            OnParametersSet();
            task = OnParametersSetAsync();
        }
        catch (Exception ex) when (ex is not NavigationException)
        {
            Debugger.BreakForUserUnhandledException(ex);
            throw;
        }

        var shouldAwaitTask = task.Status != TaskStatus.RanToCompletion &&
            task.Status != TaskStatus.Canceled;

        if (parameters)
            StateHasChanged();

        return shouldAwaitTask
            ? CallStateHasChangedOnAsyncCompletion(task, parameters) 
            : Task.CompletedTask;
    }

    //TODO
    [DebuggerDisableUserUnhandledExceptions]
    private async Task CallStateHasChangedOnAsyncCompletion(Task task, bool callStateHasChanged)
    {
        try
        {
            await task;
        }
        catch 
        {
            if (task.IsCanceled)
            {
                return;
            }

            throw;
        }

        if (callStateHasChanged)
            StateHasChanged();
    }

    private void QueueRendering()
    {
        renderHandle.Render(renderFragment);
    }

    void IRefreshable.Refresh()
    {
        InvokeAsync(StateHasChanged);
    }

    void IRefreshable.Dispose(AbstractSignal signal)
    {
        accessTracker.Untrack(signal);
    }

    //TODO
    Task IHandleEvent.HandleEventAsync(EventCallbackWorkItem callback, object? arg)
    {
        var task = callback.InvokeAsync(arg);
        var shouldAwaitTask = task.Status != TaskStatus.RanToCompletion &&
            task.Status != TaskStatus.Canceled;

        StateHasChanged();

        return shouldAwaitTask 
            ? CallStateHasChangedOnAsyncCompletion(task, PreserveDefaultRenderingBehavior) 
            : Task.CompletedTask;
    }

    Task IHandleAfterRender.OnAfterRenderAsync()
    {
        var firstRender = !hasCalledOnAfterRender;
        hasCalledOnAfterRender = true;

        OnAfterRender(firstRender);
        return OnAfterRenderAsync(firstRender);
    }
}
