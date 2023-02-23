// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Components;

public partial class DateTimeFormat
{
    [Parameter]
    public DateTimeOffset Value
    {
        get => _value;
        set
        {
            if (_value != value)
            {
                _value = value;
                if (_afterRender)
                {
                    FormartValueString();
                }
            }
        }
    }

    [Parameter]
    public string Format { get; set; } = "yyyy-MM-dd HH:mm:ss";

    private string _valueFormatString = string.Empty;

    private DateTimeOffset _value;

    private bool _afterRender = false;

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            FormartValueString();

            _afterRender = true;
        }
        return base.OnAfterRenderAsync(firstRender);
    }

    private Task FormartValueString()
    {
        if (Value != DateTimeOffset.MinValue)
        {
            _valueFormatString = Value.ToOffset(JsInitVariables.TimezoneOffset).ToString(Format);
        }
        else
        {
            _valueFormatString = "";
        }

        StateHasChanged();

        return Task.CompletedTask;
    }
}
