﻿using BlazorLighthouse.Internal;
using BlazorLighthouse.Internal.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorLighthouse.Core;

/// <summary>
/// Base for Blazor components that should subscribe to signal value changes 
/// </summary>
public class LighthouseComponentBase : SignalingContext, IComponent, IRefreshable
{
    private readonly RenderFragment renderFragment;
    private readonly AccessTracker accessTracker;
    private readonly Lock parametersChangedLockObject = new();
    private readonly Lock renderingQueueLockObject = new();

    private RenderHandle renderHandle;
    private IReadOnlyDictionary<string, object?>? parameters;

    internal bool IsRenderingQueued { get; private set; } = false;

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
        if (HaveParamtersChanged(parameters))
            StateHasChanged();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Build the render tree
    /// </summary>
    /// <param name="builder">Builder provided by the renderer</param>
    protected virtual void BuildRenderTree(RenderTreeBuilder builder)
    {

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
    /// Re-render the component
    /// </summary>
    protected void StateHasChanged()
    {
        if (!SetRenderingQueued())
            return;

        QueueRendering();
    }

    private void TrackAndBuildRenderTree(RenderTreeBuilder builder)
    {
        accessTracker.Track(() =>
        {
            IsRenderingQueued = false;
            BuildRenderTree(builder);
        });
    }

    private bool HaveParamtersChanged(ParameterView parameters)
    {
        lock (parametersChangedLockObject)
        {
            return HaveParametersChangedSync(parameters);
        }
    }

    private bool HaveParametersChangedSync(ParameterView parameters)
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

    private bool SetRenderingQueued()
    {
        lock (renderingQueueLockObject)
        {
            return SetRenderingQueuedSync();
        }
    }

    private bool SetRenderingQueuedSync()
    {
        if (IsRenderingQueued)
            return false;

        IsRenderingQueued = true;
        return true;
    }

    private void QueueRendering()
    {
        renderHandle.Render(renderFragment);
    }

    void IRefreshable.Refresh()
    {
        if (!SetRenderingQueued())
            return;

        InvokeAsync(QueueRendering);
    }

    void IRefreshable.Dispose(AbstractSignal signal)
    {
        accessTracker.Untrack(signal);
    }
}
