// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.SchedulerResources;

public partial class SchedulerResourceFiles
{
    private const string PROJECT_PREFIX = "Project_";

    private const string APP_PREFIX = "App_";

    private StringNumber _selectedProjectId = null!;

    public StringNumber SelectedProjectId
    {
        get
        {
            return _selectedProjectId;
        }
        set
        {
            if (_selectedProjectId != value)
            {
                _selectedProjectId = value;
            }
        }
    }

    private StringNumber? _selectedIdentity;
    private string _lastSelectedAppIdentity = string.Empty;

    public StringNumber? SelectedIdentity
    {
        get
        {
            return _selectedIdentity;
        }
        set
        {
            if (_selectedIdentity != value)
            {
                _selectedIdentity = value;

                if (_selectedIdentity != null && _selectedIdentity.IsT0 && _selectedIdentity.AsT0.StartsWith(APP_PREFIX))
                {
                    LastSelectedAppIdentity = _selectedIdentity.AsT0.Replace(APP_PREFIX, "");

                    SelectedResourceId = Guid.Empty.ToString();

                    _isAdd = false;
                }
            }
        }
    }

    private StringNumber _selectedResourceId = default!;

    private SchedulerResourceDto? _selectedResourceDto;

    public StringNumber SelectedResourceId
    {
        get => _selectedResourceId;
        set
        {
            if (_selectedResourceId != value)
            {
                _selectedResourceId = value;

                _selectedResourceDto = _resources.FirstOrDefault(r => r.Id.ToString() == SelectedResourceId.AsT0);

                _isAdd = false;
            }
        }
    }

    private List<SideBarItem> _allProjects = new();

    private List<ProjectAppDto> _allApp = new();

    private List<SideBarItem> _projects = new();

    private List<SchedulerResourceDto> _resources = new();

    private string Color { get; set; } = "#4318FF";

    private string _searchName = string.Empty;

    private bool _showForm = false;

    private bool _isAdd = false;

    private bool _showConfirmDialog = false;

    private string _confirmMessage = string.Empty;

    private string _confirmTitle = string.Empty;

    private Guid? _deleteResourceId;

    private string _deleteIdentityResource = string.Empty;

    private ConfirmDialogTypes _confirmType;

    private SchedulerResourceDto Model { get; set; } = new();

    private MForm? _form;

    public string LastSelectedAppIdentity
    {
        get => _lastSelectedAppIdentity;
        set
        {
            if (_lastSelectedAppIdentity != value)
            {
                _lastSelectedAppIdentity = value;
                OnSelectedAppIdChanged();
            }
        }
    }

    public string SearchName
    {
        get => _searchName;
        set
        {
            if(_searchName != value)
            {
                _searchName = value;
                OnSearchNameChanged();
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await GetProjects();

        await base.OnInitializedAsync();
    }

    private async Task GetProjects()
    {
        try
        {
            var response = await SchedulerServerCaller.PmService.GetProjectListAsync(null);

            foreach (var project in response.Data)
            {
                _projects.Add(new SideBarItem()
                {
                    Identity = PROJECT_PREFIX + project.Identity,
                    Title = project.Name,
                    IsProject = true,
                    Children = project.ProjectApps.Where(p=> p.Type == ProjectAppTypes.Job).Select(app => new SideBarItem() { Identity = APP_PREFIX + app.Identity, Title = app.Name, IsProject = false }).ToList()
                });

                _allApp.AddRange(project.ProjectApps);
            }

            _allProjects = _projects;
        }
        catch (Exception ex)
        {
            await PopupService.ToastAsync(ex.Message, AlertTypes.Error);
        }
    }

    private async Task GetResourceList()
    {
        if (string.IsNullOrWhiteSpace(_lastSelectedAppIdentity))
        {
            return;
        }

        var request = new SchedulerResourceListRequest() { JobAppIdentity = _lastSelectedAppIdentity };

        var response = await SchedulerServerCaller.SchedulerResourceService.GetListAsync(request);

        _resources = response.Data;

        StateHasChanged();
    }

    private Task OnSelectedAppIdChanged()
    {
        return GetResourceList();
    }

    private StringNumber GetKey(SideBarItem item)
    {
        StringNumber key;

        if (item.IsProject)
        {
            key = PROJECT_PREFIX + item.Identity;
        }
        else
        {
            key = APP_PREFIX + item.Identity;
        }

        return key;
    }

    private Task OnSearchNameChanged()
    {
        if (string.IsNullOrEmpty(SearchName))
        {
            _projects = _allProjects;
            return Task.CompletedTask;
        }

        var parentList = new List<SideBarItem>();

        foreach (var item in _allProjects)
        {
            foreach (var child in item.Children)
            {
                if (child.Title.Contains(SearchName))
                {
                    var parent = parentList.FirstOrDefault(p => p.Identity == item.Identity);

                    if(parent == null)
                    {
                        parent = new SideBarItem()
                        {
                            Title = item.Title,
                            Identity = item.Identity,
                            Expanded = item.Expanded,
                            IsProject = item.IsProject
                        };

                        parentList.Add(parent);
                    }

                    parent.Children.Add(child);
                }
            }
        }

        _projects = parentList;

        return Task.CompletedTask;
    }

    private async Task AddResourceFile()
    {
        Model = new();
        Model.JobAppIdentity = LastSelectedAppIdentity;
        Model.Version = await GetDefaultVersion();
        _isAdd = true;
    }

    private Task<string> GetDefaultVersion()
    {
        if (!_resources.Any())
        {
            return Task.FromResult("1.0.0");
        }
        else
        {
            var lastResource = _resources.FirstOrDefault()!;
          
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

    private Task ShowDialog(ConfirmDialogTypes confirmType, Guid resourceId)
    {
        if(confirmType == ConfirmDialogTypes.DeleteResourceVersion)
        {
            var resource = _resources.FirstOrDefault(r => r.Id == resourceId);

            if(resource != null)
            {
                var app = _allApp.FirstOrDefault(p => p.Identity == resource.JobAppIdentity);

                if(app != null)
                {
                    _showConfirmDialog = true;
                    _confirmMessage = string.Format(T("DeleteResourceVersionMessage"), app.Name, resource.Version);
                    _confirmTitle = T("DeleteResourceVersion");
                    _deleteResourceId = resourceId;
                    _confirmType = confirmType;
                }
            }
        }
        else if(confirmType == ConfirmDialogTypes.DeleteResources)
        {
            var app = _allApp.FirstOrDefault(p => p.Identity == LastSelectedAppIdentity);

            if(app != null)
            {
                _showConfirmDialog = true;
                _confirmMessage = string.Format(T("DeleteResourceMessage"), app.Name);
                _confirmTitle = T("DeleteResources");
                _deleteResourceId = Guid.Empty;
                _confirmType = confirmType;
                _deleteIdentityResource = app.Identity;
            }
        }
        else
        {
            _showConfirmDialog = false;
            _confirmType = 0;
            _deleteResourceId = null;
        }

        return Task.CompletedTask;
    }

    private async Task OnSureDelete()
    {
        var deleteSuccess = false;

        if(_confirmType == ConfirmDialogTypes.DeleteResourceVersion && _deleteResourceId.HasValue)
        {
            await SchedulerServerCaller.SchedulerResourceService.DeleteAsync(new RemoveSchedulerResourceRequest() { ResourceId = _deleteResourceId.Value });

            deleteSuccess = true;
        }
        else if(_confirmType == ConfirmDialogTypes.DeleteResources)
        {
            await SchedulerServerCaller.SchedulerResourceService.DeleteAsync(new RemoveSchedulerResourceRequest() { JobAppIdentity = _deleteIdentityResource });

            deleteSuccess = true;
        }

        if (deleteSuccess)
        {
            _showConfirmDialog = false;

            await PopupService.ToastAsync("Delete success", AlertTypes.Success);

            await GetResourceList();
        }
    }

    private async Task AfterSubmit()
    {
        _isAdd = false;
        await GetResourceList();

        if (_resources.Any())
        {
            SelectedResourceId = _resources.FirstOrDefault()!.Id.ToString();
        }
    }
}
