﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.Teams.Components;

public partial class SchedulerTasks
{
    [CascadingParameter]
    public SLayout Layout { get; set; } = default!;

    [Parameter]
    public string JobId { get; set; } = string.Empty;

    [Inject]
    public IMultiEnvironmentUserContext MultiEnvironmentUserContext { get; set; } = default!;

    private Guid _jobId => string.IsNullOrEmpty(JobId) ? default : Guid.Parse(JobId);
    private TaskRunStatus _queryStatus;
    private TaskRunStatus _lastQueryStatus;
    private bool _showConfirmDialog;
    private string _confirmTitle = string.Empty;
    private string _confirmMessage = string.Empty;
    private ConfirmDialogTypes _confirmDialogType;
    private Guid _confirmTaskId;
    private List<string> _orginList = new();
    private List<JobQueryTimeTypes> _jobQueryTimeTypeList = new();
    private bool IsApiCreate => _job != null && !string.IsNullOrWhiteSpace(_job.Origin);
    private bool _showProgressbar = true;

    private Task QueryStatusChanged(TaskRunStatus status)
    {
        if (_queryStatus != status)
        {
            _queryStatus = status;
            return OnQueryDataChanged();
        }

        return Task.CompletedTask;
    }

    private JobQueryTimeTypes _queryTimeType = JobQueryTimeTypes.ScheduleTime;

    private DateTimeOffset? _queryStartTime;

    private Task QueryTimeChanged((DateTimeOffset? queryStartTime, DateTimeOffset? queryEndTime) arg)
    {
        _queryStartTime = arg.queryStartTime;
        _queryEndTime = arg.queryEndTime;
        return OnQueryDataChanged();
    }

    private DateTimeOffset? _queryEndTime;

    public int Page
    {
        get => _page;
        set
        {
            if (_page != value)
            {
                _page = value;
                OnQueryDataChanged(false);
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

    bool _showTraceDialog = false;
    bool _showLogDialog = false;
    SchedulerTaskDto? _current = default;

    protected async override Task OnInitializedAsync()
    {
        await MasaSignalRClient.HubConnectionBuilder();

        MasaSignalRClient.HubConnection?.On(SignalRMethodConsts.GET_NOTIFICATION, async (SchedulerTaskDto schedulerTaskDto, string env) =>
        {
            if (MultiEnvironmentUserContext.Environment == env)
            {
                await SignalRNotifyDataHandler(schedulerTaskDto);
            }
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

        _queryStatusList = GetEnumMap<TaskRunStatus>();
        _queryStatusList.RemoveAll(p => p.Value == TaskRunStatus.Idle);

        _jobQueryTimeTypeList = Enum.GetValues<JobQueryTimeTypes>().Where(t => t != JobQueryTimeTypes.CreationTime && t != JobQueryTimeTypes.ModificationTime).ToList();

        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            _job = await SchedulerServerCaller.SchedulerJobService.GetAsync(_jobId);
            await OnQueryDataChanged();
        }

        Layout.ReplaceLastBreadcrumb(T("Task"));
        Layout.UpdateBreadcrumbs(items =>
        {
            items[0].Href = $"/job?isState={true}";
        });
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

    private Task OnQueryDataChanged(bool resetPage = true)
    {
        return GetTaskListAsync(resetPage);
    }

    private async Task GetTaskListAsync(bool resetPage = true)
    {
        if (_job is null)
        {
            _showProgressbar = false;
            PopupService.HideProgressLinear();
            return;
        }

        _showProgressbar = true;
        PopupService.ShowProgressLinear();
        var request = new SchedulerTaskListRequest()
        {
            JobId = _job.Id,
            FilterStatus = _queryStatus,
            Origin = _queryOrigin,
            Page = Page,
            PageSize = PageSize,
            QueryEndTime = _queryEndTime?.UtcDateTime,
            QueryStartTime = _queryStartTime?.UtcDateTime,
            QueryTimeType = _queryTimeType
        };

        var response = await SchedulerServerCaller.SchedulerTaskService.GetListAsync(request);

        _total = response.Total;

        _tasks = response.Data;

        _orginList = response.OriginList;

        if (resetPage)
        {
            Page = 1;
        }

        _showProgressbar = false;
        PopupService.HideProgressLinear();
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
            totalSecond = (DateTimeOffset.UtcNow - taskStartTime).TotalSeconds;
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
                await PopupService.EnqueueSnackbarAsync("Confirm type eror", AlertTypes.Error);
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
                PopupService.EnqueueSnackbarAsync("Confirm type eror", AlertTypes.Error);
                break;
        }

        return Task.CompletedTask;
    }

    private void ResetQueryOptions()
    {
        _queryStatus = default;
        _lastQueryStatus = default;
        _queryTimeType = JobQueryTimeTypes.ScheduleTime;
        _queryStartTime = default;
        _queryEndTime = default;
        _page = 1;
        _pageSize = 10;
        _queryOrigin = string.Empty;
    }

    private bool CheckNotifyData(SchedulerTaskDto? task)
    {
        if (task == null)
        {
            return false;
        }

        if (_job == null)
        {
            return false;
        }

        if (task.JobId != _job.Id)
        {
            return false;
        }

        switch (_queryTimeType)
        {
            case JobQueryTimeTypes.ScheduleTime:
                if (_queryStartTime.HasValue && task.SchedulerTime < _queryStartTime)
                {
                    return false;
                }

                if (_queryEndTime.HasValue && task.SchedulerTime >= _queryEndTime)
                {
                    return false;
                }
                break;
            case JobQueryTimeTypes.RunStartTime:
                if (_queryStartTime.HasValue && task.TaskRunStartTime < _queryStartTime)
                {
                    return false;
                }

                if (_queryEndTime.HasValue && task.TaskRunStartTime >= _queryEndTime)
                {
                    return false;
                }
                break;
            case JobQueryTimeTypes.RunEndTime:
                if (_queryStartTime.HasValue && task.TaskRunEndTime < _queryStartTime)
                {
                    return false;
                }

                if (_queryEndTime.HasValue && task.TaskRunEndTime >= _queryEndTime)
                {
                    return false;
                }
                break;
        }

        if (!string.IsNullOrWhiteSpace(_queryOrigin) && !task.Origin.Contains(_queryOrigin))
        {
            return false;
        }

        if (_queryStatus != 0 && task.TaskStatus != _queryStatus)
        {
            return false;
        }

        return true;
    }

    private async Task SignalRNotifyDataHandler(SchedulerTaskDto taskDto)
    {
        if (CheckNotifyData(taskDto))
        {
            var index = _tasks.FindIndex(j => j.Id == taskDto.Id);

            if (index > -1)
            {
                var task = _tasks.ElementAt(index);
                taskDto.OperatorName = task.OperatorName;
                _tasks[index] = taskDto;
            }
            else if (Page == 1)
            {
                var sameOperatorJob = _tasks.FirstOrDefault(j => j.OperatorId == taskDto.OperatorId);

                if (sameOperatorJob != null)
                {
                    taskDto.OperatorName = sameOperatorJob.OperatorName;
                }
                else
                {
                    var userInfo = await SchedulerServerCaller.AuthService.GetUserInfoAsync(taskDto.OperatorId);
                    taskDto.OperatorName = userInfo.Name;
                }

                _tasks.Add(taskDto);

                _tasks = _tasks.OrderByDescending(p => p.SchedulerTime).OrderByDescending(p => p.CreationTime).Take(_pageSize).ToList();
            }

            StateHasChanged();
        }
    }

    private void OpenTaskLog(SchedulerTaskDto item)
    {
        _current = item;
        _showLogDialog = true;
        StateHasChanged();
    }

    private void OpenTaskTrace(SchedulerTaskDto item)
    {
        _current = item;
        _showTraceDialog = true;
        StateHasChanged();
    }
}