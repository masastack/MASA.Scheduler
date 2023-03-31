// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin;

public abstract class ProCompontentBase : BDomComponentBase
{
    private SchedulerServerCaller? _schedulerServerCaller;
    private GlobalConfig? _globalConfig;
    private NavigationManager? _navigationManager;

    [Inject]
    public I18n I18n { get; set; } = default!;

    [Inject]
    public SchedulerServerCaller SchedulerServerCaller
    {
        get
        {
            return _schedulerServerCaller ?? throw new Exception("please Inject SchedulerCaller!");
        }
        set
        {
            _schedulerServerCaller = value;
        }
    }

    [Inject]
    public GlobalConfig GlobalConfig
    {
        get
        {
            return _globalConfig ?? throw new Exception("please Inject GlobalConfig!");
        }
        set
        {
            _globalConfig = value;
        }
    }

    [Inject]
    public NavigationManager NavigationManager
    {
        get
        {
            return _navigationManager ?? throw new Exception("please Inject NavigationManager!");
        }
        set
        {
            _navigationManager = value;
        }

    }

    [Inject]
    public IMapper Mapper { get; set; } = default!;

    [Inject]
    public IPopupService PopupService { get; set; } = default!;

    [Inject]
    public MasaSignalRClient MasaSignalRClient { get; set; } = default!;

    [Inject]
    public IUserContext UserContext { get; set; } = default!;

    [Inject]
    public JsInitVariables JsInitVariables { get; set; } = default!;

    [CascadingParameter(Name = "Culture")]
    private string Culture { get; set; } = null!;

    public List<KeyValuePair<string, TEnum>> GetEnumMap<TEnum>() where TEnum : struct, Enum
    {
        return Enum.GetValues<TEnum>().Select(e => new KeyValuePair<string, TEnum>(e.ToString(), e)).ToList();
    }

    protected virtual string? PageName { get; set; }

    public string T(string key)
    {
        if (string.IsNullOrEmpty(key)) return key;
        if (PageName is not null) return I18n?.T(PageName, key, false) ?? I18n?.T(key, false) ?? key;
        else return I18n?.T(key, true) ?? key;
    }

    public string T(string formatkey, params string[] args)
    {
        return string.Format(T(formatkey), args);
    }

    public void OpenInformationMessage(string message)
    {
        PopupService.EnqueueSnackbarAsync(message, AlertTypes.Info);
    }

    public void OpenSuccessMessage(string message)
    {
        PopupService.EnqueueSnackbarAsync(message, AlertTypes.Success);
    }

    public void OpenWarningMessage(string message)
    {
        PopupService.EnqueueSnackbarAsync(message, AlertTypes.Warning);
    }

    public void OpenErrorMessage(string message)
    {
        PopupService.EnqueueSnackbarAsync(message, AlertTypes.Error);
    }

    public async Task ConfirmAsync(string messgae, Func<Task> callback, AlertTypes type = AlertTypes.Warning)
    {
        if (await PopupService.SimpleConfirmAsync(T("OperationConfirmation"), messgae, type)) await callback.Invoke();
    }

    public async Task ConfirmAsync(string title, string messgae, Func<Task> callback, AlertTypes type = AlertTypes.Warning)
    {
        if (await PopupService.SimpleConfirmAsync(title, messgae, type)) await callback.Invoke();
    }


    protected string GetRunTimeDescription(double runTime)
    {
        if (runTime < 0)
        {
            return "--";
        }

        var min = ((int)runTime) / 60;

        var second = runTime % 60;

        if (min > 0)
        {
            return $"{min}m {second:0.##}s";
        }
        else
        {
            return $"{second:0.##}s";
        }
    }
}
