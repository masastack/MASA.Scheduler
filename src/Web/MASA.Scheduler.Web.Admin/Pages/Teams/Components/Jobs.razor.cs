﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.Teams.Components;

public partial class Jobs
{
    [Parameter]
    public int ProjectId
    {
        get
        {
            return _projectId;
        }
        set
        {
            if (_projectId != value)
            {
                _projectId = value;

                OnProjectChanged();
            }
        }
    }

    private int _projectId;

    private TaskRunStatus _queryStatus;

    private string _queryJobName = string.Empty;

    private JobQueryTimeTypes _jobQueryType;

    private DateTime? _queryStartTime;

    private DateTime? _queryEndTime;

    private bool _showFilter;

    public List<KeyValuePair<string, JobQueryTimeTypes>> JobQueryTimeTypes { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        JobQueryTimeTypes = GetEnumMap<JobQueryTimeTypes>();

        await base.OnInitializedAsync();
    }

    private string ComputedStatusColor(TaskRunStatus status)
    {
        switch (status)
        {
            case TaskRunStatus.Success:
                return "#05CD99";
            case TaskRunStatus.Failure:
                return "#FF5252";
            case TaskRunStatus.Timeout:
                return "#FF7D00";
            case TaskRunStatus.TimeoutSuccess:
                return "#CC9139";
            default:
                return "#323D6F";
        }
    }

    public Task ShowFilter()
    {
        _showFilter = !_showFilter;
        return Task.CompletedTask;
    }

    public Task OnProjectChanged()
    {
        return Task.CompletedTask;
    }

}

