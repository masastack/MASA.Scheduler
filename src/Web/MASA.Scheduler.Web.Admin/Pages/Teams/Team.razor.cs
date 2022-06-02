// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.Teams;

public partial class Team
{
    [Parameter]
    public string TeamId { get; set; } = string.Empty;

    private ProjectDto _project = default!;
    private StringNumber _curTab = 0;
    private Dictionary<StringNumber, string> NavTab => new Dictionary<StringNumber, string>() { { 0, T("Job") }, { 1, T("Task") } };

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        return base.OnAfterRenderAsync(firstRender);
    }

    public Task OnProjectChangedAsync(ProjectDto project)
    {
        _project = project;
        return Task.CompletedTask;
    }
}
