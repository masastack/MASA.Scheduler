// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.Teams.Components;
public partial class Projects
{
    [Parameter]
    public Guid? TeamId
    {
        get
        {
            return _teamId;
        }
        set 
        { 
            if(_teamId != value)
            {
                _teamId = value;
                OnTeamChangeAsync();
            }
        }
    }

    [Parameter]
    public EventCallback<ProjectDto> OnProjectChanged { get; set; }

    private string _projectName = string.Empty;
    private List<ProjectDto> _projects = new();
    private StringNumber _selectedProjectIdentity = null!;
    private Guid? _teamId = null;

    public StringNumber SelectedProjectIdentity
    {
        get 
        { 
            return _selectedProjectIdentity; 
        }
        set
        {
            if(_selectedProjectIdentity != value)
            {
                _selectedProjectIdentity = value;

                if (OnProjectChanged.HasDelegate)
                {
                    var project = _projects.FirstOrDefault(p => p.Identity == _selectedProjectIdentity.AsT0);

                    OnProjectChanged.InvokeAsync(project);
                }
            }
        }
    }

    private Task OnTeamChangeAsync()
    {
        return GetProjectList();
    }

    private async Task GetProjectList()
    {
        if(_teamId == null)
        {
            _projects = new();
            return;
        }

        _projects = (await SchedulerServerCaller.PmService.GetProjectListAsync(_teamId.Value)).Data;

        if (_projects.Any())
        {
            SelectedProjectIdentity = _projects.FirstOrDefault()!.Identity;
        }
        else
        {
            SelectedProjectIdentity = string.Empty;
        }

        StateHasChanged();
    }
}

