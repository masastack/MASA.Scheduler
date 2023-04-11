// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.SchedulerResources;

public partial class SchedulerResourceFiles
{
    private List<ProjectDto> _projects = new();

    private StringNumber _selectedProjectIdentity = string.Empty;

    private List<AppResourceViewModel> _resourceData = new();

    private bool _showConfirmDialog;

    private string _confirmMessage = string.Empty;

    private string _confirmTitle = string.Empty;

    private Guid? _deleteResourceId;

    private string _deleteIdentityResource = string.Empty;

    private ConfirmDialogTypes _confirmType;

    private SchedulerResourceDto Model { get; set; } = new();

    private SchedulerResourceFilesInformation? _resourceInfoModal;

    private AddSchedulerResourceFiles? _addResourceModal;

    private bool _showProgressbar = true;

    private Guid _teamId = default;

    [Inject]
    public Stack.Components.Configs.GlobalConfig StackGlobalConfig { get; set; } = default!;

    protected async override Task OnInitializedAsync()
    {
        _teamId = StackGlobalConfig.CurrentTeamId;

        await GetProjects();

        StackGlobalConfig.OnCurrentTeamChanged += CurrentTeamChanged;

        await base.OnInitializedAsync();
    }

    private void CurrentTeamChanged(Guid teamId)
    {
        _teamId = teamId;
        GetProjects().ContinueWith(_ => InvokeAsync(StateHasChanged));
    }

    private async Task GetProjects()
    {
        try
        {
            var response = await SchedulerServerCaller.PmService.GetProjectListAsync(_teamId);
            _projects = response.Data.Where(x => x.ProjectApps.Any(pa => pa.Type == ProjectAppTypes.Job)).ToList();

            var defaultProject = _projects.FirstOrDefault();
            _selectedProjectIdentity = defaultProject?.Identity ?? string.Empty;
            await GetResourceData();
        }
        catch (Exception ex)
        {
            await PopupService.EnqueueSnackbarAsync(ex.Message, AlertTypes.Error);
        }
    }

    private async Task HandleProjectIdentityChanged(StringNumber val)
    {
        _selectedProjectIdentity = val;
        await GetResourceData();
    }

    private async Task GetResourceData()
    {
        _showProgressbar = true;
        var project = _projects.FirstOrDefault(x => x.Identity == _selectedProjectIdentity);
        if (project == null) {
            _showProgressbar = false;
            return; 
        }
        var jobs = project.ProjectApps.Where(p => p.Type == ProjectAppTypes.Job).ToList();
        var resources = new List<AppResourceViewModel>();
        foreach (var job in jobs)
        {
            var request = new SchedulerResourceListRequest() { JobAppIdentity = job.Identity };
            var response = await SchedulerServerCaller.SchedulerResourceService.GetListAsync(request);
            var resourceDtos = response.Data;

            resources.Add(new AppResourceViewModel
            {
                Identity = job.Identity,
                Name = job.Name,
                Resources = resourceDtos
            });
        }
        _resourceData = resources;
        _showProgressbar = false;
    }

    private async Task ShowDialog(ConfirmDialogTypes confirmType, Guid resourceId, string appIdentity)
    {
        if (confirmType == ConfirmDialogTypes.DeleteResourceVersion)
        {
            var app = _resourceData.FirstOrDefault(x => x.Resources.Any(r => r.Id == resourceId));
            var resource = app?.Resources.FirstOrDefault(x => x.Id == resourceId);

            if (resource != null)
            {
                _confirmMessage = string.Format(T("DeleteResourceVersionMessage"), app?.Name, resource.Version);
                _confirmTitle = T("DeleteResourceVersion");
                _deleteResourceId = resourceId;
                _confirmType = confirmType;

                await ConfirmAsync(_confirmTitle, _confirmMessage, OnSureDelete, AlertTypes.Error);
            }
        }
        else if (confirmType == ConfirmDialogTypes.DeleteResources)
        {
            var app = _resourceData.FirstOrDefault(p => p.Identity == appIdentity);

            if (app != null)
            {
                _confirmMessage = string.Format(T("DeleteResourceMessage"), app.Name);
                _confirmTitle = T("DeleteResources");
                _deleteResourceId = Guid.Empty;
                _confirmType = confirmType;
                _deleteIdentityResource = app.Identity;

                await ConfirmAsync(_confirmTitle, _confirmMessage, OnSureDelete, AlertTypes.Error);
            }
        }
        else
        {
            _showConfirmDialog = false;
            _confirmType = 0;
            _deleteResourceId = null;
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task OnSureDelete()
    {
        var deleteSuccess = false;

        if (_confirmType == ConfirmDialogTypes.DeleteResourceVersion && _deleteResourceId.HasValue)
        {
            await SchedulerServerCaller.SchedulerResourceService.DeleteAsync(new RemoveSchedulerResourceRequest() { ResourceId = _deleteResourceId.Value });

            deleteSuccess = true;
        }
        else if (_confirmType == ConfirmDialogTypes.DeleteResources)
        {
            await SchedulerServerCaller.SchedulerResourceService.DeleteAsync(new RemoveSchedulerResourceRequest() { JobAppIdentity = _deleteIdentityResource });

            deleteSuccess = true;
        }

        if (deleteSuccess)
        {
            _showConfirmDialog = false;

            await PopupService.EnqueueSnackbarAsync(T("DeleteSuccess"), AlertTypes.Success);

            await GetResourceData();
        }
    }

    private async Task AddResourceFile(string appIdentity)
    {
        Model = new();
        Model.JobAppIdentity = appIdentity;
        Model.Version = await GetDefaultVersion();
        _addResourceModal?.OpenModalAsync(Model);
    }

    private Task<string> GetDefaultVersion()
    {
        var app = _resourceData.FirstOrDefault(p => p.Identity == Model.JobAppIdentity);
        var resources = app?.Resources ?? new();
        if (!resources.Any())
        {
            return Task.FromResult("1.0.0");
        }
        else
        {
            var lastResource = resources.FirstOrDefault()!;

            var lastVersionArr = lastResource.Version.Split(".");

            if (int.TryParse(lastVersionArr.Last(), out var lastVersionNumber))
            {
                lastVersionNumber += 1;
                lastVersionArr[lastVersionArr.Count() - 1] = lastVersionNumber.ToString();

                return Task.FromResult(string.Join(".", lastVersionArr));
            }
        }
        return Task.FromResult(string.Empty);
    }
}
