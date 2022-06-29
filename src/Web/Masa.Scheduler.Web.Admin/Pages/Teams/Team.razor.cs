// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.Teams;

public partial class Team
{
    [Parameter]
    public string TeamId { get; set; } = string.Empty;

    private bool JobVisible => _curTab == 0;

    private bool TaskVisible => _curTab == 1;

    private ProjectDto _project = default!;
    private SchedulerJobDto? _selectedJob;
    private StringNumber _curTab = 0;
    private Dictionary<StringNumber, string> NavTab => new Dictionary<StringNumber, string>() { { 0, T("Job") }, { 1, T("Task") } };

    public Task OnProjectChangedAsync(ProjectDto project)
    {
        _project = project;
        _curTab = 0;
        return Task.CompletedTask;
    }

    public Task HandleJobSelect(SchedulerJobDto job)
    {
        _selectedJob = job;
        _curTab = 1;
        return Task.CompletedTask;
    }
}
