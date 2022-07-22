// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.SchedulerResources.Components;

public partial class SchedulerResourceFilesInformation
{
    [Parameter]
    public SchedulerResourceDto Model
    {
        get
        {
            return _model;
        }
        set
        {
            _model = value;
            GetDescription();
        }
    }

    private string _description = string.Empty;

    private SchedulerResourceDto _model = new();

    protected override async Task OnInitializedAsync()
    {
        await GetDescription();
        await base.OnInitializedAsync();
    }

    private async Task GetDescription()
    {
        var creatorName = "Tester";

        if (Model.Creator != Guid.Empty)
        {
            var userInfo = await SchedulerServerCaller.AuthService.GetUserInfoAsync(Model.Creator);

            if (userInfo != null)
            {
                creatorName = userInfo.Name;
            }
        }

        var uploadTime = Model.UploadTime.ToOffset(GlobalConfig.TimezoneOffset).ToString(T("$DateTimeFormat"));

        _description = string.Format(T("ResourceFileUploadDescription"), creatorName, uploadTime);
    }


    private async Task Download()
    {
        if (string.IsNullOrEmpty(Model.FilePath))
        {
            await PopupService.ToastAsync("File path is empty", AlertTypes.Error);
            return;
        }

        NavigationManager.NavigateTo(Model.FilePath);
    }
}

