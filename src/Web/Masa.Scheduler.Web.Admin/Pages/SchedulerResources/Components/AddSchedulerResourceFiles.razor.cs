// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Masa.BuildingBlocks.Ddd.Domain.Entities;

namespace Masa.Scheduler.Web.Admin.Pages.SchedulerResources.Components;

public partial class AddSchedulerResourceFiles
{
    [Parameter]
    public EventCallback OnAfterSubmit { get; set; }

    [Parameter]
    public List<SchedulerResourceDto> Resources { get; set; } = new();

    public SchedulerResourceDto Model { get; set; } = new();

    private MForm? _form;

    private MFileInput<IBrowserFile> _ref = default!;

    private IBrowserFile _browserFile = default!;

    private List<Func<IBrowserFile, StringBoolean>> _rules = default!;

    private bool _visible;

    private int _progress = 0;

    private bool? _isUploadSuccess;

    protected override Task OnInitializedAsync()
    {
        _rules = new List<Func<IBrowserFile, StringBoolean>>()
        {
            value=> value != null ? true :  T("FileIsRequired"),
            value=> (value != null && value.Size<1024*1024*100) ? true : T("FileSizeNotValid")
        };
        return base.OnInitializedAsync();
    }
    private async Task Submit()
    {
        MasaArgumentException.ThrowIfNull(_form, "form");

        if (!_form.Validate())
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Model.FilePath))
        {
            await PopupService.ToastAsync(T("PleaseUploadFiles"), AlertTypes.Error);
            return;
        }

        if (_form.Validate())
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

            _visible = false;

            ResetForm();

            if (OnAfterSubmit.HasDelegate)
            {
                await OnAfterSubmit.InvokeAsync();
            }
        }
    }

    private async void HandleFileChange(IBrowserFile file)
    {
        _progress = 0;
        if (file == null)
        {
            _isUploadSuccess = false;
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
            _isUploadSuccess = false;
            return;
        }

        if(file.Size > 100 * 1024 * 1024)
        {
            await PopupService.ToastAsync(T("FileSizeNotValid"), AlertTypes.Error);
            _isUploadSuccess = false;
            return;
        }

        _browserFile = file;

        var securityToken = await SchedulerServerCaller.OssService.GetSecurityTokenAsync();

        if(securityToken == null)
        {
            await PopupService.ToastAsync(T("GetOssTokenFailed"), AlertTypes.Error);
            _isUploadSuccess = false;
            return;
        }

        Model.Name = file.Name;

        var fileName = Guid.NewGuid() + "-" + file.Name;

        var uploadUrl = await JsInvokeAsync<string>("ossUpload", _ref.Ref, securityToken, fileName);

        if(uploadUrl == null)
        {
            await PopupService.ToastAsync(T("UploadFileFailed"), AlertTypes.Error);
            _isUploadSuccess = false;
            return;
        }

        Model.FilePath = uploadUrl;

        Model.UploadTime = DateTimeOffset.Now;

        _progress = 100;
        _isUploadSuccess = true;
    }

    private void HandleVisibleChanged(bool val)
    {
        if (!val) HandleCancel();
    }

    private void HandleCancel()
    {
        _visible = false;
        ResetForm();
    }

    private void ResetForm()
    {
        Model = new();
    }

    public async Task OpenModalAsync(SchedulerResourceDto model)
    {
        Model= model;

        await InvokeAsync(() =>
        {
            _visible = true;
            StateHasChanged();
        });

        _form?.ResetValidation();
    }
}
