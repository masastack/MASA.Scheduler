// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.Teams.Components;
public partial class Projects
{
    [Parameter]
    public Guid TeamId { get; set; } = Guid.Empty;

    [Parameter]
    public EventCallback<int> OnProjectChanged { get; set; }

    private string _projectName = string.Empty;
    private List<ProjectModel> _projects = new();
    private StringNumber _selectedProjectId = null!;

    public StringNumber SelectedProjectId
    {
        get 
        { 
            return _selectedProjectId; 
        }
        set
        {
            if(_selectedProjectId != value)
            {
                _selectedProjectId = value;

                if (OnProjectChanged.HasDelegate)
                {
                    OnProjectChanged.InvokeAsync(_selectedProjectId.AsT1);
                }
            }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitDataAsync();
        }
    }

    private async Task InitDataAsync()
    {
        _projects = await SchedulerCaller.PMService.GetProjectListAsync(TeamId);
        SelectedProjectId = SelectedProjectId == 0 && _projects.Any() ? _projects.FirstOrDefault()!.Id : SelectedProjectId;
        StateHasChanged();
    }
}

