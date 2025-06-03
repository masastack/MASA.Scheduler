// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Components.Tsc;

public partial class ApmSamplePage
{
    [Parameter]
    public int Total { get; set; }

    [Parameter]
    public int Current { get; set; }

    [Parameter]
    public EventCallback<int> CurrentChanged { get; set; }

    private async Task OnNextAsync()
    {
        if (Total - Current < 1)
            return;

        Current += 1;
        if (CurrentChanged.HasDelegate)
            await CurrentChanged.InvokeAsync(Current);
    }

    private async Task OnPreAsync()
    {
        if (Current - 1 <= 0)
            return;

        Current -= 1;
        if (CurrentChanged.HasDelegate)
            await CurrentChanged.InvokeAsync(Current);
    }

    private async Task OnPageAsync(int value)
    {
        if (value < 1 || value - Total > 0 || value - Current == 0)
            return;

        Current = value;
        if (CurrentChanged.HasDelegate)
            await CurrentChanged.InvokeAsync(Current);
    }
}
