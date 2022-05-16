// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin
{
    public abstract class ProCompontentBase : ComponentBase
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
        public SchedulerServerCaller SchedulerCaller
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

        public bool Loading
        {
            get => GlobalConfig.Loading;
            set => GlobalConfig.Loading = value;
        }

        public string T(string key)
        {
            return LanguageProvider.T(key) ?? key;
        }
    }
}