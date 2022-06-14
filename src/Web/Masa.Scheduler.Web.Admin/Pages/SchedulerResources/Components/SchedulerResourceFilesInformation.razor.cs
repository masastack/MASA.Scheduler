// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.SchedulerResources.Components;

public partial class SchedulerResourceFilesInformation
{
    [Parameter]
    public SchedulerResourceDto Model { get; set; } = new();

    private string GetUploadDescription()
    {
        var creatorName = "Tester";
        var description = string.Format(T("ResourceFileUploadDescription"), creatorName, Model.UploadTime.ToString(T("$DateTimeFormat")));
        return description;
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

