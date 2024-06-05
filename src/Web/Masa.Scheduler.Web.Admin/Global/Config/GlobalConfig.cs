
// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.
namespace Masa.Scheduler.Web.Admin.Global;

public class GlobalConfig
{
    #region Field

    private bool _loading;

    #endregion

    #region Property

    public bool Loading
    {
        get => _loading;
        set
        {
            if (_loading != value)
            {
                _loading = value;
                OnLoadingChanged?.Invoke(_loading, LoadingText);
            }
        }
    }

    public string LoadingText { get; set; } = "Loading";

    #endregion

    #region event

    public delegate void GlobalConfigChanged();
    public delegate void LoadingChanged(bool loading, string loadingText);

    public event GlobalConfigChanged? OnCurrentNavChanged;
    public event LoadingChanged? OnLoadingChanged;

    #endregion
}
