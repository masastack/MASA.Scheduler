// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.Teams;

public partial class Team
{
    [Parameter]
    public string TeamId { get; set; } = string.Empty;

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
        if (string.IsNullOrEmpty(TeamId))
        {
            _teamId = StackGlobalConfig.CurrentTeamId;
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnParametersSet()
    {
        _teamId = string.IsNullOrEmpty(TeamId) ? StackGlobalConfig.CurrentTeamId : Guid.Parse(TeamId);
    }

    public Task HandleJobSelect(SchedulerJobDto job)
    {
        _selectedJob = job;
        _curTab = 1;
        return Task.CompletedTask;
    }
}
