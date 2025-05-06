using BlazorLighthouse.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorLighthouseTest.Core;

public partial class LighthouseComponentBaseTest
{
    internal class TestComponent(Action buildRenderTree) : LighthouseComponentBase
    {
        [Parameter]
        public object? Property1 { get; set; }
        [Parameter]
        public object? Property2 { get; set; }

        public Task ExecuteInvokeAsync(Action action)
        {
            return InvokeAsync(action);
        }

        public void ExecuteStateHasChanged()
        {
            StateHasChanged();
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);
            buildRenderTree();
        }
    }
}
