// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.Teams.Components;

public partial class SchedulerTasks
{
    [Parameter]
    public SchedulerJobDto? SelectedJob
    {
        get
        {
            return _job;
        }
        set
        {
            if (_job?.Id != value?.Id)
            {
                _job = value;
                _jobChange = true;
                OnQueryDataChanged();
            }
            else
            {
                _jobChange = false;
            }
        }
    }

    [Parameter]
    public bool Visible
    {
        get
        {
            return _visible;
        }
        set
        {
            if(_visible != value)
            {
                _visible = value;

                if (_visible && !_jobChange)
                {
                    OnQueryDataChanged();
                }
            }
        }
    }

    private bool _jobChange;
    private bool _visible;
    private TaskRunStatus _queryStatus;
    private TaskRunStatus _lastQueryStatus;
    private bool _showConfirmDialog;
    private string _confirmTitle = string.Empty;
    private string _confirmMessage = string.Empty;
    private ConfirmDialogTypes _confirmDialogType;
    private Guid _confirmTaskId;

    private Task QueryStatusChanged(TaskRunStatus status)
    {
        if(_queryStatus != status)
        {
            _queryStatus = status;
            return OnQueryDataChanged();
        }

        return Task.CompletedTask;
    }

    private JobQueryTimeTypes _queryTimeType;

    private DateTime? _queryStartTime;

    private Task QueryStartTimeChanged(DateTime? queryStartTime)
    {
        if(_queryStartTime != queryStartTime)
        {
            _queryStartTime = queryStartTime;
            return OnQueryDataChanged();
        }

        return Task.CompletedTask;
    }

    private DateTime? _queryEndTime;

    private Task QueryEndTimeChanged(DateTime? queryEndTime)
    {
        if(_queryEndTime != queryEndTime)
        {
            _queryEndTime = queryEndTime;
            return OnQueryDataChanged();
        }

        return Task.CompletedTask;
    }

    public int Page
    {
        get => _page; 
        set
        {
            if (_page != value)
            {
                _page = value;
                OnQueryDataChanged();
            }
        }
    }

    private int _page = 1;

    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (_pageSize != value)
            {
                _pageSize = value;
                OnQueryDataChanged();
            }
        }
    }

    private int _pageSize = 10;

    private long _total;

    private Task QueryOriginChanged(string queryOrigin)
    {
        _queryOrigin = queryOrigin;
        return OnQueryDataChanged();
    }

    private string _queryOrigin = string.Empty;

    private SchedulerJobDto? _job;

    public List<DataTableHeader<SchedulerTaskDto>> Headers { get; set; } = new();

    private List<SchedulerTaskDto> _tasks = new();

    private List<KeyValuePair<string, TaskRunStatus>> _queryStatusList = new();

    protected override async Task OnInitializedAsync()
    {
        await MasaSignalRClient.HubConnectionBuilder();

        MasaSignalRClient.HubConnection?.On(SignalRMethodConsts.GET_NOTIFICATION, async () =>
        {
            await GetTaskListAsync();
        });

        Headers = new()
        {
            new() { Text = T(nameof(SchedulerTaskDto.SchedulerTime)), Value = nameof(SchedulerTaskDto.SchedulerTime), Sortable = false },
            new() { Text = T(nameof(SchedulerTaskDto.TaskRunStartTime)), Value = nameof(SchedulerTaskDto.TaskRunStartTime), Sortable = false },
            new() { Text = T(nameof(SchedulerTaskDto.TaskRunEndTime)), Value = nameof(SchedulerTaskDto.TaskRunEndTime), Sortable = false },
            new() { Text = T(nameof(SchedulerTaskDto.TaskStatus)), Value = nameof(SchedulerTaskDto.TaskStatus), Sortable = false },
            new() { Text = T(nameof(SchedulerTaskDto.RunTime)), Value = nameof(SchedulerTaskDto.RunTime), Sortable = false },
            new() { Text = T(nameof(SchedulerTaskDto.RunCount)), Value = nameof(SchedulerTaskDto.RunCount), Sortable = false },
            new() { Text = T(nameof(SchedulerTaskDto.Origin)), Value = nameof(SchedulerTaskDto.Origin), Sortable = false },
            new() { Text = T("SchedulerTask.OperatorId"), Value = nameof(SchedulerTaskDto.OperatorId), Sortable = false },
            new() { Text = T("Action"), Value = "Action", Sortable = false },
        };

        if (_job != null && string.IsNullOrWhiteSpace(_job.Origin))
        {
            Headers.RemoveAll(p => p.Value == nameof(SchedulerTaskDto.Origin));
        }

        _queryStatusList = GetEnumMap<TaskRunStatus>();

        _queryStatusList.RemoveAll(p => p.Value == TaskRunStatus.Idle);

        await base.OnInitializedAsync();
    }

    private string ComputedStatusColor(TaskRunStatus status)
    {
        switch (status)
        {
            case TaskRunStatus.Success:
                return "#05CD99 !important";
            case TaskRunStatus.Failure:
                return "#FF5252 !important";
            case TaskRunStatus.Timeout:
                return "#FF7D00 !important";
            case TaskRunStatus.TimeoutSuccess:
                return "#CC9139 !important";
            default:
                return "#323D6F !important";
        }
    }

    private Task OnQueryDataChanged()
    {
        return GetTaskListAsync();
    }

    private async Task GetTaskListAsync()
    {
        if (_job is null)
        {
            return;
        }

        DateTimeOffset? queryEndTimeDateTimeOffset = _queryEndTime.HasValue ? new DateTimeOffset(_queryEndTime.Value, TimezoneOffset) : null;
        DateTimeOffset? queryStartTimeDateTimeOffset = _queryStartTime.HasValue ? new DateTimeOffset(_queryStartTime.Value, TimezoneOffset) : null;

        var request = new SchedulerTaskListRequest()
        {
            JobId = _job.Id,
            FilterStatus = _queryStatus,
            Origin = _queryOrigin,
            Page = Page,
            PageSize = PageSize,
            QueryEndTime = queryEndTimeDateTimeOffset.HasValue ? queryEndTimeDateTimeOffset.Value.ToLocalTime().DateTime : null,
            QueryStartTime = queryStartTimeDateTimeOffset.HasValue ? queryStartTimeDateTimeOffset.Value.ToLocalTime().DateTime : null,
            QueryTimeType = _queryTimeType
        };

        var response = await SchedulerServerCaller.SchedulerTaskService.GetListAsync(request);

        _total = response.Total;

        _tasks = response.Data;

        StateHasChanged();
    }

    private string GetRunTime(DateTimeOffset taskStartTime, DateTimeOffset taskEndTime)
    {
        double totalSecond;

        if (taskStartTime != DateTimeOffset.MinValue && taskEndTime != DateTimeOffset.MinValue)
        {
            totalSecond = (taskEndTime - taskStartTime).TotalSeconds;
        }
        else if (taskStartTime != DateTimeOffset.MinValue)
        {
            totalSecond = (DateTimeOffset.Now - taskStartTime).TotalSeconds;
        }
        else
        {
            totalSecond = -1;
        }

        return GetRunTimeDescription(totalSecond);
    }

    private async Task StartTask(Guid taskId)
    {
        var request = new StartSchedulerTaskRequest()
        {
            OperatorId = UserContext.GetUserId<Guid>(),
            IsManual = true,
            TaskId = taskId
        };

        await SchedulerServerCaller.SchedulerTaskService.StartAsync(request);

        await GetTaskListAsync();

        OpenSuccessMessage(T("RestartTaskSuccess"));
    }

    private async Task DeleteTask(Guid taskId)
    {
        var request = new RemoveSchedulerTaskRequest()
        {
            OperatorId = UserContext.GetUserId<Guid>(),
            TaskId = taskId
        };

        await SchedulerServerCaller.SchedulerTaskService.RemoveAsync(request);

        await GetTaskListAsync();

        OpenSuccessMessage(T("DeleteTaskSuccess"));
    }

    private async Task StopTask(Guid taskId)
    {
        var request = new StopSchedulerTaskRequest()
        {
            OperatorId = UserContext.GetUserId<Guid>(),
            TaskId = taskId
        };

        await SchedulerServerCaller.SchedulerTaskService.StopAsync(request);

        OpenSuccessMessage(T("StopTaskSuccess"));
    }

    private Task RadioGroupClickHandler()
    {
        if (_lastQueryStatus == _queryStatus)
        {
            QueryStatusChanged(0);
        }

        _lastQueryStatus = _queryStatus;

        return Task.CompletedTask;
    }

    private async Task OnSure()
    {
        switch (_confirmDialogType)
        {
            case ConfirmDialogTypes.DeleteTask:
                await DeleteTask(_confirmTaskId);
                break;
            case ConfirmDialogTypes.StopTask:
                await StopTask(_confirmTaskId);
                break;
            case ConfirmDialogTypes.RestartTask:
                await StartTask(_confirmTaskId);
                break;
            default:
                await PopupService.ToastErrorAsync("Confirm type eror");
                break;
        }

        _showConfirmDialog = false;
    }

    private Task OnShowConfirmDialog(ConfirmDialogTypes confirmDialogType, Guid taskId)
    {
        _confirmTaskId = taskId;
        _confirmDialogType = confirmDialogType;
        _showConfirmDialog = true;

        switch (confirmDialogType)
        {
            case ConfirmDialogTypes.DeleteTask:
                _confirmMessage = T("DeleteTaskConfirmMessage");
                _confirmTitle = T("DeleteTask");
                break;
            case ConfirmDialogTypes.StopTask:
                _confirmMessage = T("StopTaskConfirmMessage");
                _confirmTitle = T("StopTask");
                break;
            case ConfirmDialogTypes.RestartTask:
                _confirmMessage = T("RestartTaskConfirmMessage");
                _confirmTitle = T("RestartTask");
                break;
            default:
                _showConfirmDialog = false;
                _confirmTaskId = Guid.Empty;
                PopupService.ToastErrorAsync("Confirm type eror");
                break;
        }

        return Task.CompletedTask;
    }
}

