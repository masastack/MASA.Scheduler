﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Global
{
    public class GlobalConfig
    {
        #region Field

        private bool _isDark;
        private string? _pageMode;
        private bool _expandOnHover;
        private bool _navigationMini;
        private string? _favorite;
        private NavModel? _currentNav;
        private CookieStorage? _cookieStorage;
        private bool _loading;

        #endregion

        #region Property

        public static string IsDarkCookieKey { get; set; } = "GlobalConfig_IsDark";

        public static string PageModeKey { get; set; } = "GlobalConfig_PageMode";

        public static string NavigationMiniCookieKey { get; set; } = "GlobalConfig_NavigationMini";

        public static string ExpandOnHoverCookieKey { get; set; } = "GlobalConfig_ExpandOnHover";

        public static string FavoriteCookieKey { get; set; } = "GlobalConfig_Favorite";

        public I18nConfig? I18nConfig { get; set; }

        public string? Language
        {
            get => I18nConfig?.Language;
            set
            {
                if (I18nConfig is not null)
                {
                    I18nConfig.Language = value;
                    OnLanguageChanged?.Invoke();
                }
            }
        }

        public bool IsDark
        {
            get => _isDark;
            set
            {
                _isDark = value;
                _cookieStorage?.SetItemAsync(IsDarkCookieKey, value);
            }
        }

        public string PageMode
        {
            get => _pageMode ?? PageModes.PageTab;
            set
            {
                _pageMode = value;
                _cookieStorage?.SetItemAsync(PageModeKey, value);
                OnPageModeChanged?.Invoke();
            }
        }

        public bool NavigationMini
        {
            get => _navigationMini;
            set
            {
                _navigationMini = value;
                _cookieStorage?.SetItemAsync(NavigationMiniCookieKey, value);
            }
        }

        public bool ExpandOnHover
        {
            get => _expandOnHover;
            set
            {
                _expandOnHover = value;
                _cookieStorage?.SetItemAsync(ExpandOnHoverCookieKey, value);
            }
        }

        public string? Favorite
        {
            get => _favorite;
            set
            {
                _favorite = value;
                _cookieStorage?.SetItemAsync(FavoriteCookieKey, value);
            }
        }

        public NavModel? CurrentNav
        {
            get => _currentNav;
            set
            {
                _currentNav = value;
                OnCurrentNavChanged?.Invoke();
            }
        }

        public bool Loading
        {
            get => _loading;
            set
            {
                if (_loading != value)
                {
                    _loading = value;
                    OnLoadingChanged?.Invoke(_loading);
                }
            }
        }

        #endregion

        public GlobalConfig(CookieStorage cookieStorage, I18nConfig i18nConfig, IHttpContextAccessor httpContextAccessor)
        {
            _cookieStorage = cookieStorage;
            I18nConfig = i18nConfig;
            if (httpContextAccessor.HttpContext is not null) Initialization(httpContextAccessor.HttpContext.Request.Cookies);
        }

        #region event

        public delegate void GlobalConfigChanged();
        public delegate void LoadingChanged(bool Loading);
        public event GlobalConfigChanged? OnPageModeChanged;
        public event GlobalConfigChanged? OnCurrentNavChanged;
        public event GlobalConfigChanged? OnLanguageChanged;
        public event LoadingChanged? OnLoadingChanged;

        #endregion

        #region Method

        public void Initialization(IRequestCookieCollection cookies)
        {
            _isDark = Convert.ToBoolean(cookies[IsDarkCookieKey]);
            _pageMode = cookies[PageModeKey];
            _navigationMini = Convert.ToBoolean(cookies[NavigationMiniCookieKey]);
            _expandOnHover = Convert.ToBoolean(cookies[ExpandOnHoverCookieKey]);
            _favorite = cookies[FavoriteCookieKey];
        }
        #endregion
    }
}