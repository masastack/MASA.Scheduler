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
            if (_teamId != value)
            {
                _teamId = value;
                OnTeamChangeAsync();
            }
        }
    }

    [Parameter]
    public string ProjectIdentity { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<ProjectDto> OnProjectChanged { get; set; }

    [Inject]
    public SchedulerJobsState SchedulerJobsState { get; set; } = default!;

    private List<ProjectDto> _projects = new();
    private string _selectedProjectIdentity = string.Empty;
    private Guid? _teamId = null;

    public string SelectedProjectIdentity
    {
        get
        {
            return _selectedProjectIdentity;
        }
        set
        {
            if (_selectedProjectIdentity != value)
            {
                _selectedProjectIdentity = value;
                SchedulerJobsState.ProjectIdentity = value;

                if (OnProjectChanged.HasDelegate)
                {
                    var project = _projects.FirstOrDefault(p => p.Identity == _selectedProjectIdentity);

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
        if (_teamId == null || _teamId == Guid.Empty)
        {
            _projects = new();
            return;
        }

        _projects = (await SchedulerServerCaller.PmService.GetProjectListAsync(_teamId.Value)).Data;

        if (_projects.Any())
        {
            var project = _projects.FirstOrDefault(x => x.Identity == ProjectIdentity);
            if (project == null)
                project = _projects.FirstOrDefault();

            NextTick(async () =>
            {
                await Task.Delay(1);
                SelectedProjectIdentity = project!.Identity;
            });
            if (project != null)
            {
                await OnProjectChanged.InvokeAsync(project);
            }
        }
        else
        {
            SelectedProjectIdentity = string.Empty;
        }

        StateHasChanged();
    }
}

