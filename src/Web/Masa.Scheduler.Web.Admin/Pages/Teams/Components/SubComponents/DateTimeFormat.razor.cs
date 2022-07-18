// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.Teams.Components.SubComponents;

public partial class DateTimeFormat
{
    [Parameter]
    public DateTimeOffset Value { get; set; }

    [Parameter]
    public string Format { get; set; } = "yyyy-MM-dd HH:mm:ss";

    private string _valueFormatString = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        if(Value != DateTimeOffset.MinValue)
        {
            var localTimeStr = await JsInvokeAsync<string>("toLocalTime", Value);

            var localDateTime = Convert.ToDateTime(localTimeStr);

            _valueFormatString = localDateTime.ToString(Format);
        }

        await base.OnInitializedAsync();
    }
}
