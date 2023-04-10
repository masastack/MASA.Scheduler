// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.Teams;

public partial class Team
{
    [Parameter]
    public string TeamId { get; set; } = string.Empty;

    [Inject]
    public MasaUser CurrentUser { get; set; } = default!;

    [Inject]
    public Stack.Components.Configs.GlobalConfig StackGlobalConfig { get; set; } = default!;

    private Guid _teamId = default;

    private bool JobVisible => _curTab == 0;
    private bool TaskVisible => _curTab == 1;
    private SchedulerJobDto? _selectedJob;
    private StringNumber _curTab = 0;
    private string _jobTabName = string.Empty;

    protected async override Task OnInitializedAsync()
    {
        _jobTabName = T("Job");

        await base.OnInitializedAsync();
    }

    protected async override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            SetCurrentTeamId(TeamId);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnParametersSet()
    {
        SetCurrentTeamId(TeamId);
    }

    private void SetCurrentTeamId(string? teamId)
    {
        if (string.IsNullOrEmpty(teamId))
        {
            _teamId = StackGlobalConfig.CurrentTeamId;
            if (_teamId == Guid.Empty)
            {
                //StackGlobalConfig.CurrentTeamId will only init after component `User` render
                _teamId = CurrentUser.CurrentTeamId;
            }
        }
        else
        {
            _teamId = Guid.Parse(teamId);
        }
    }

    public Task HandleJobSelect(SchedulerJobDto job)
    {
        _selectedJob = job;
        _curTab = 1;
        return Task.CompletedTask;
    }
}
