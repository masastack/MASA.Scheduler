// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin;

public abstract class ProCompontentBase : BDomComponentBase
{
    private I18n? _languageProvider;
    private SchedulerServerCaller? _schedulerServerCaller;
    private GlobalConfig? _globalConfig;
    private NavigationManager? _navigationManager;

    [Inject]
    public I18n LanguageProvider
    {
        get
        {
            return _languageProvider ?? throw new Exception("please Inject I18n!");
        }
        set
        {
            _languageProvider = value;
        }
    }

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

    public List<KeyValuePair<string, TEnum>> GetEnumMap<TEnum>() where TEnum : struct, Enum
    {
        return Enum.GetValues<TEnum>().Select(e => new KeyValuePair<string, TEnum>(e.ToString(), e)).ToList();
    }

    public string T(string key)
    {
        return LanguageProvider.T(key) ?? key;
    }

    public async Task<bool> OpenConfirmDialog(string content)
    {
        return await PopupService.ConfirmAsync(T("Operation confirmation"), content, AlertTypes.Error);
    }

    public async Task<bool> OpenConfirmDialog(string title, string content)
    {
        return await PopupService.ConfirmAsync(title, content);
    }

    public async Task<bool> OpenConfirmDialog(string title, string content, AlertTypes type)
    {
        return await PopupService.ConfirmAsync(title, content, type);
    }

    public void OpenInformationMessage(string message)
    {
        PopupService.ToastInfoAsync(message);
    }

    public void OpenSuccessMessage(string message)
    {
        PopupService.ToastSuccessAsync(message);
    }

    public void OpenWarningMessage(string message)
    {
        PopupService.ToastWarningAsync(message);
    }

    public void OpenErrorMessage(string message)
    {
        PopupService.ToastErrorAsync(message);
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
