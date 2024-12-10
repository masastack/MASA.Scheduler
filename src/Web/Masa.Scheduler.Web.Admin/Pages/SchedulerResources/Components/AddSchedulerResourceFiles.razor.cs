// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

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

    private IBrowserFile? _browserFile;

    private List<Func<IBrowserFile, StringBoolean>> _rules = default!;

    private bool _visible;

    private int _progress = 0;

    private bool? _isUploadSuccess;

    private IJSObjectReference UploadJs = default!;

    protected override Task OnInitializedAsync()
    {
        _rules = new List<Func<IBrowserFile, StringBoolean>>()
        {
            value=> value != null ? true :  T("FileIsRequired"),
            value=> (value != null && value.Size<1024*1024*100) ? true : T("FileSizeNotValid")
        };
        return base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            UploadJs = await Js!.InvokeAsync<IJSObjectReference>("import", "./_content/Masa.Stack.Components/js/upload/upload.js");
        }
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
            await PopupService.EnqueueSnackbarAsync(T("PleaseUploadFiles"), AlertTypes.Error);
            return;
        }

        if (_form.Validate())
        {
            if (Resources.Any(p => p.Version == Model.Version))
            {
                await PopupService.EnqueueSnackbarAsync(T("VersionAlreadyExists"), AlertTypes.Error);
                return;
            }

            var request = new AddSchedulerResourceRequest()
            {
                Data = Model
            };

            await SchedulerServerCaller.SchedulerResourceService.AddAsync(request);

            await PopupService.EnqueueSnackbarAsync(T("AddResourceSuccess"), AlertTypes.Success);

            _visible = false;

            ResetForm();

            if (OnAfterSubmit.HasDelegate)
            {
                await OnAfterSubmit.InvokeAsync();
            }
        }
    }

    private async Task HandleFileChange(IBrowserFile file)
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
            await PopupService.EnqueueSnackbarAsync(T("FileNotValid"), AlertTypes.Error);
            _isUploadSuccess = false;
            return;
        }

        if(file.Size > 100 * 1024 * 1024)
        {
            await PopupService.EnqueueSnackbarAsync(T("FileSizeNotValid"), AlertTypes.Error);
            _isUploadSuccess = false;
            return;
        }

        _browserFile = file;

        var securityToken = await SchedulerServerCaller.OssService.GetSecurityTokenAsync();

        if(securityToken == null)
        {
            await PopupService.EnqueueSnackbarAsync(T("GetOssTokenFailed"), AlertTypes.Error);
            _isUploadSuccess = false;
            return;
        }

        Model.Name = file.Name;

        var fileName = Guid.NewGuid() + "-" + file.Name;

        var uploadUrls = await UploadJs!.InvokeAsync<List<string>>("InputFileUpload", _ref.InputFile?.Element, "UploadImage", securityToken);

        if(uploadUrls == null || !uploadUrls.Any())
        {
            await PopupService.EnqueueSnackbarAsync(T("UploadFileFailed"), AlertTypes.Error);
            _isUploadSuccess = false;
            return;
        }

        Model.FilePath = uploadUrls[0];

        Model.UploadTime = DateTimeOffset.UtcNow;

        _progress = 100;
        _isUploadSuccess = true;
        StateHasChanged();
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
        _progress = 0;
        _isUploadSuccess = null;
    }

    public async Task OpenModalAsync(SchedulerResourceDto model)
    {
        Model = model;
        _browserFile = null;
        await InvokeAsync(() =>
        {
            _visible = true;
            StateHasChanged();
        });

        _form?.ResetValidation();
    }
}
