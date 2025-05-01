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
    : SignalingContext, IComponent, IRefreshable, IHandleEvent, IHandleAfterRender
{
    private readonly RenderFragment renderFragment;
    private readonly AccessTracker accessTracker;

    private RenderHandle renderHandle;
    private (IComponentRenderMode? mode, bool cached) renderMode;

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
        if (this.renderHandle.IsInitialized)
            throw new InvalidOperationException($"Render handle is already set");

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
        var callStateHasChanged = EnforceStateHasChanged();

        if (!initialized)
        {
            initialized = true;
            return CallInitAndSetParametersAsync(callStateHasChanged);
        }

        return CallOnParametersSetAsync(callStateHasChanged);
    }

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

        QueueRendering();
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

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected virtual bool EnforceStateHasChanged() {
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
            hasPendingQueuedRender = true;
            renderHandle.Render(renderFragment);
        }
        catch
        {
            hasPendingQueuedRender = false;
            throw;
        }
    }

    void IRefreshable.Refresh()
    {
        InvokeAsync(StateHasChanged);
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
