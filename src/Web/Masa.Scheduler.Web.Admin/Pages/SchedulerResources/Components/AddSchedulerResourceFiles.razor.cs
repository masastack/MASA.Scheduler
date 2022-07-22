// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.SchedulerResources.Components;

public partial class AddSchedulerResourceFiles
{
    [Parameter]
    public SchedulerResourceDto Model { get; set; } = new();

    [Parameter]
    public EventCallback OnAfterSubmit { get; set; }

    [Parameter]
    public List<SchedulerResourceDto> Resources { get; set; } = new();

    private MForm? _form;

    private MFileInput<IBrowserFile> _ref = default!;

    private IBrowserFile _browserFile = default!;

    private List<Func<IBrowserFile, StringBoolean>> _rules = default!;

    protected override Task OnInitializedAsync()
    {
        _rules = new List<Func<IBrowserFile, StringBoolean>>()
        {
            value=> value != null ? true :  T("FileIsRequired"),
            value=> (value != null && value.Size<1024*1024*100) ? true : T("FileSizeNotValid")
        };
        return base.OnInitializedAsync();
    }
    private async Task Submit(EditContext context)
    {
        if(await _ref.ValidateAsync(true))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Model.FilePath))
        {
            await PopupService.ToastAsync(T("PleaseUploadFiles"), AlertTypes.Error);
            return;
        }

        if (context.Validate())
        {
            if (Resources.Any(p => p.Version == Model.Version))
            {
                await PopupService.ToastAsync(T("VersionAlreadyExists"), AlertTypes.Error);
                return;
            }

            var request = new AddSchedulerResourceRequest()
            {
                Data = Model
            };

            await SchedulerServerCaller.SchedulerResourceService.AddAsync(request);

            await PopupService.ToastAsync(T("AddResourceSuccess"), AlertTypes.Success);

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
            await PopupService.ToastAsync(T("FileIsEmpty"), AlertTypes.Error);
            return;
        }

        var filterExtension = new List<string>()
        {
            ".zip",
            ".dll"
        };

        if(!filterExtension.Contains(Path.GetExtension(file.Name)))
        {
            await PopupService.ToastAsync(T("FileNotValid"), AlertTypes.Error);
            return;
        }

        if(file.Size > 100 * 1024 * 1024)
        {
            await PopupService.ToastAsync(T("FileSizeNotValid"), AlertTypes.Error);
            return;
        }

        _browserFile = file;

        var securityToken = await SchedulerServerCaller.OssService.GetSecurityTokenAsync();

        if(securityToken == null)
        {
            await PopupService.ToastAsync(T("GetOssTokenFailed"), AlertTypes.Error);
            return;
        }

        Model.Name = file.Name;

        var fileName = Guid.NewGuid() + "-" + file.Name;

        var uploadUrl = await JsInvokeAsync<string>("ossUpload", _ref.Ref, securityToken, fileName);

        Model.FilePath = uploadUrl;

        Model.UploadTime = DateTimeOffset.Now;
    }
}
