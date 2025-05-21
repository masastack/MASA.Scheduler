// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Components.Tsc;

public partial class MonacoEditor
{
    [Parameter]
    public string Value { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    [Parameter]
    public bool ReadOnly { get; set; }

    [Parameter]
    public string Language { get; set; } = "json";

    [Parameter]
    public string Height { get; set; } = "312px";

    private MMonacoEditor? _editor;
    private readonly StandaloneThemeData _theme = new StandaloneThemeData
    {
        Base = "vs",
        inherit = true,
        rules = Array.Empty<TokenThemeRule>(),
        colors = new Dictionary<string, string>
            {
                { "editor.background", "#F6F8FD" },
            }
    };
    private bool _isRendered;

    private Task<object> InitOptions()
    {
        object options = new
        {
            theme = "vs",
            automaticLayout = true,
            language = Language,
            readOnly = ReadOnly
        };

        return Task.FromResult(options);
    }
    private Action? _updateReadOnlyAction = null;


    public override async Task SetParametersAsync(ParameterView parameters)
    {
        var readOnly = parameters.GetValueOrDefault<bool>(nameof(ReadOnly));
        if (_editor != null && readOnly != ReadOnly)
        {
            _updateReadOnlyAction = async () =>
            {
                await _editor.UpdateOptionsAsync(new
                {
                    readOnly = readOnly
                });
            };
            _updateReadOnlyAction.Invoke();
        }

        await base.SetParametersAsync(parameters);
    }


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _isRendered = true;
        }

        if (_isRendered && _editor != null)
        {
            await _editor.DefineThemeAsync("theme", _theme);
            await _editor.SetThemeAsync("theme");

            _isRendered = false;
        }
    }
}