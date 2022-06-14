// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.SchedulerResources.Components;

public partial class AddSchedulerResourceFiles
{
    [Parameter]
    public SchedulerResourceDto Model { get; set; } = new();

    [Parameter]
    public EventCallback OnAfterSubmit { get; set; }

    private MForm? _form;

    private MFileInput<IBrowserFile> _ref = default!;

    private IBrowserFile? _browserFile;

    private async Task Submit(EditContext context)
    {
        if (context.Validate())
        {
            var request = new AddSchedulerResourceRequest()
            {
                Data = Model
            };

            await SchedulerServerCaller.SchedulerResourceService.AddAsync(request);

            await PopupService.ToastAsync("Add resource files Success", AlertTypes.Success);

            if (OnAfterSubmit.HasDelegate)
            {
                await OnAfterSubmit.InvokeAsync();
            }
        }
    }

    private async void HandleFileChange(IBrowserFile file)
    {
        if(file == null)
        {
            await PopupService.ToastAsync("Upload file is empty", AlertTypes.Error);
            return;
        }

        _browserFile = file;

        var securityToken = await SchedulerServerCaller.OssService.GetSecurityTokenAsync();

        if(securityToken == null)
        {
            await PopupService.ToastAsync("Upload file is empty", AlertTypes.Error);
            return;
        }

        Model.Name = file.Name;

        var uploadUrl = await JsInvokeAsync<string>("ossUpload", _ref.Ref, securityToken);

        Model.FilePath = uploadUrl;

        Model.UploadTime = DateTimeOffset.Now;
    }

    private List<Func<IBrowserFile, StringBoolean>> _rules = new List<Func<IBrowserFile, StringBoolean>>()
    {
        value=> value != null ? true : "Files is required",
        value=> (value != null && value.Size<1024*1024*100) ? true : "files size should be less than 100 MB!"
    };
}
