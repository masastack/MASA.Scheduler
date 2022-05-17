// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.Teams.Components;

public partial class Jobs
{
    private int _projectId;

    [Parameter]
    public int ProjectId
    {
        get
        {
            return _projectId;
        }
        set
        {
            if(_projectId != value)
            {
                _projectId = value;

                OnProjectChanged();
            }
        }
    }

    public Task OnProjectChanged()
    {
        return Task.CompletedTask;
    }
}

