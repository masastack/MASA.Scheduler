// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.Teams.Components;

public partial class SchedulerJobs : ProComponentBase
{
    [Parameter]
    public Guid? TeamId { get; set; }

    public ProjectDto? Project
    {
        get => _project;
        set
        {
            if (_project?.Id != value?.Id)
            {
                _project = value;
                _projectChange = true;
                ResetQueryOptions();
                OnQueryDataChanged();
            }
            else
            {
                _projectChange = false;
            }
        }
    }

    [Parameter]
    public EventCallback<SchedulerJobDto> OnJobSelect { get; set; }

    [Parameter]
    public bool Visible
    {
        get => _visible;
        set
        {
            if (_visible != value)
            {
                _visible = value;
                if (_visible && !_projectChange)
                {
                    OnQueryDataChanged();
                }
            }
        }
    }

    private bool _projectChange;

    private bool _visible;

    private ProjectDto? _project = default;

    private TaskRunStatus _queryStatus;

    private TaskRunStatus _lastQueryStatus;

    private string _queryJobName = string.Empty;

    private JobQueryTimeTypes _queryTimeType = JobQueryTimeTypes.CreationTime;

    private JobCreateTypes _jobCreateType = JobCreateTypes.Manual;

    private int _page = 1;

    private int _pageSize = 10;

    private long _total;

    private JobTypes _queryJobType;

    private string _queryOrigin = string.Empty;

    private List<SchedulerJobDto> _jobs = new();

    private SchedulerJobDto _modalModel = new();

    private List<KeyValuePair<string, TaskRunStatus>> _queryStatusList = new();

    private List<JobQueryTimeTypes> _jobQueryTimeTypeList = new();

    private bool _showProgressbar = true;

    private bool _showConfirmDialog;

    private string _confirmTitle = string.Empty;

    private string _confirmMessage = string.Empty;

    private ConfirmDialogTypes _confirmDialogType;

    private Guid _confirmJobId = Guid.Empty;

    private List<string> _originList = new();

    private JobModal? _jobModal;

    public TaskRunStatus QueryStatus
    {
        get => _queryStatus;
        set
        {
            if (_queryStatus != value)
            {
                _queryStatus = value;
                OnQueryDataChanged();
            }
        }
    }

    public string QueryJobName
    {
        get => _queryJobName;
        set
        {
            if (_queryJobName != value)
            {
                _queryJobName = value;
                OnQueryDataChanged();
            }
        }
    }

    public JobQueryTimeTypes QueryTimeType
    {
        get => _queryTimeType;
        set
        {
            if (_queryTimeType != value)
            {
                _queryTimeType = value;
                OnQueryDataChanged();
            }
        }
    }

    public DateTime? QueryStartTime { get; set; }

    public DateTime? QueryEndTime { get; set; }

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

    public JobTypes QueryJobType
    {
        get => _queryJobType;
        set
        {
            if (_queryJobType != value)
            {
                _queryJobType = value;
                OnQueryDataChanged();
            }
        }
    }

    public string QueryOrigin
    {
        get => _queryOrigin;
        set
        {
            if (_queryOrigin != value)
            {
                _queryOrigin = value;
                OnQueryDataChanged();
            }
        }
    }

    protected async override Task OnInitializedAsync()
    {
        await MasaSignalRClient.HubConnectionBuilder();

        MasaSignalRClient.HubConnection?.On(SignalRMethodConsts.GET_NOTIFICATION, async (SchedulerTaskDto schedulerTaskDto) =>
        {
            await SignalRNotifyDataHandler(schedulerTaskDto);
        });

        _queryStatusList = GetEnumMap<TaskRunStatus>();
        _queryStatusList.RemoveAll(p => p.Value == TaskRunStatus.WaitToRun);

        _jobQueryTimeTypeList = Enum.GetValues<JobQueryTimeTypes>().Where(t => t == JobQueryTimeTypes.CreationTime || t == JobQueryTimeTypes.ModificationTime).ToList();

        await base.OnInitializedAsync();
    }

    private async Task SwitchJobCreateType(JobCreateTypes jobCreateTypes)
    {
        if (_jobCreateType != jobCreateTypes)
        {
            _jobCreateType = jobCreateTypes;
            ResetQueryOptions();
            await GetProjectJobs();
        }
    }

    public Task OnQueryDataChanged(bool resetPage = true)
    {
        return GetProjectJobs(resetPage);
    }

    private async Task GetProjectJobs(bool resetPage = true)
    {
        if (Project == null)
        {
            _jobs = new();
            _total = 0;
            _showProgressbar = false;
            return;
        }

        _showProgressbar = true;

        var request = new SchedulerJobListRequest()
        {
            FilterStatus = QueryStatus,
            IsCreatedByManual = _jobCreateType == JobCreateTypes.Manual,
            JobName = QueryJobName,
            JobType = QueryJobType,
            Origin = QueryOrigin,
            Page = Page,
            PageSize = PageSize,
            QueryEndTime = QueryEndTime?.Add(JsInitVariables.TimezoneOffset),
            QueryStartTime = QueryStartTime?.Add(JsInitVariables.TimezoneOffset),
            QueryTimeType = QueryTimeType,
            BelongProjectIdentity = Project.Identity,
        };

        var jobListResponse = await SchedulerServerCaller.SchedulerJobService.GetListAsync(request);

        _total = jobListResponse.Total;

        _jobs = jobListResponse.Data;

        _originList = jobListResponse.OriginList;

        if (resetPage)
        {
            _page = 1;
        }

        _showProgressbar = false;
        StateHasChanged();
    }

    private string GetJobColor(SchedulerJobDto job)
    {
        switch (job.LastRunStatus)
        {
            case TaskRunStatus.Idle:
                return "#A3AED0";
            case TaskRunStatus.Success:
                return "#00B42A";
            case TaskRunStatus.Failure:
                return "#FF5252";
            case TaskRunStatus.Timeout:
                return "#FF7D00";
            case TaskRunStatus.TimeoutSuccess:
                return "#FF7D00";
            default:
                return "#485585";
        }
    }

    private string GetJobRunText(SchedulerJobDto job)
    {
        switch (job.LastRunStatus)
        {
            case TaskRunStatus.Idle:
                return T("Idle");
            case TaskRunStatus.Running:
                var runTime = (DateTimeOffset.Now - job.LastRunStartTime).TotalSeconds;
                return T("AlreadyRun") + GetRunTimeDescription(runTime);
            case TaskRunStatus.WaitToRetry:
                return T("WaitToRetry");
            case TaskRunStatus.Success:
            case TaskRunStatus.Failure:
            case TaskRunStatus.Timeout:
            case TaskRunStatus.TimeoutSuccess:
                return TimeSpan.FromMilliseconds((DateTime.UtcNow - job.LastRunEndTime).TotalMilliseconds).Humanize(culture: I18n.Culture, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Year) + T("Ago") + T(job.LastRunStatus.ToString());
        }

        return "";
    }

    private async Task RunJob(SchedulerJobDto dto)
    {
        var startJobRequest = new StartSchedulerJobRequest()
        {
            JobId = dto.Id,
            OperatorId = UserContext.GetUserId<Guid>()
        };

        await SchedulerServerCaller.SchedulerJobService.StartJobAsync(startJobRequest);

        await PopupService.EnqueueSnackbarAsync(T("RequestSuccess"), AlertTypes.Success);

        await GetProjectJobs();
    }

    private async Task AddJob()
    {
        if (Project == null)
        {
            await PopupService.EnqueueSnackbarAsync(T("Project is null"), AlertTypes.Warning);
            return;
        }

        _modalModel = new();
        _modalModel.BelongProjectIdentity = Project.Identity;
        _modalModel.BelongTeamId = Project.TeamId;
        _modalModel.Enabled = true;

        await _jobModal?.OpenModalAsync(_modalModel)!;
    }

    private async Task EditJob(SchedulerJobDto dto)
    {
        _modalModel = Mapper.Map<SchedulerJobDto>(dto);

        await _jobModal?.OpenModalAsync(_modalModel)!;
    }

    private async Task DisabledJob(Guid jobId)
    {
        var request = new ChangeEnabledStatusRequest()
        {
            JobId = jobId,
            Enabled = false
        };

        await SchedulerServerCaller.SchedulerJobService.ChangeEnableStatusAsync(request);

        await PopupService.EnqueueSnackbarAsync(T("RequestSuccess"), AlertTypes.Success);

        await GetProjectJobs();
    }

    private async Task EnabledJob(Guid jobId)
    {
        var request = new ChangeEnabledStatusRequest()
        {
            JobId = jobId,
            Enabled = true
        };

        await SchedulerServerCaller.SchedulerJobService.ChangeEnableStatusAsync(request);

        await PopupService.EnqueueSnackbarAsync(T("RequestSuccess"), AlertTypes.Success);

        await GetProjectJobs();
    }

    public async Task OnAfterDataChange()
    {
        await GetProjectJobs();
    }

    public async Task HandleJobSelect(SchedulerJobDto job)
    {
        if (OnJobSelect.HasDelegate)
        {
            await OnJobSelect.InvokeAsync(job);
        }
    }

    private Task RadioGroupClickHandler()
    {
        if (_lastQueryStatus == QueryStatus)
        {
            QueryStatus = 0;
        }

        _lastQueryStatus = QueryStatus;

        return Task.CompletedTask;
    }

    private Task ShowDialog(ConfirmDialogTypes confirmDialogType, Guid jobId)
    {
        _confirmJobId = jobId;
        _confirmDialogType = confirmDialogType;
        var job = _jobs.FirstOrDefault(p => p.Id == jobId);
        if (job == null)
        {
            _showConfirmDialog = false;
            _confirmJobId = Guid.Empty;
            _confirmDialogType = 0;
            PopupService.EnqueueSnackbarAsync("JobId error", AlertTypes.Error);
            return Task.CompletedTask;
        }

        switch (confirmDialogType)
        {
            case ConfirmDialogTypes.EnabledJob:
                _confirmMessage = string.Format(T("EnabledJobConfirmMessage"), job.Name);
                _confirmTitle = T("EnabledJob");
                break;
            case ConfirmDialogTypes.DisabledJob:
                _confirmMessage = string.Format(T("DisabledJobConfirmMessage"), job.Name);
                _confirmTitle = T("DisabledJob");
                break;
            default:
                _showConfirmDialog = false;
                _confirmJobId = Guid.Empty;
                PopupService.EnqueueSnackbarAsync("Confirm type error", AlertTypes.Error);
                break;
        }

        return ConfirmAsync(_confirmTitle, _confirmMessage, OnSure, AlertTypes.Warning);
    }

    private async Task OnSure()
    {
        if (_confirmJobId == Guid.Empty)
        {
            _confirmDialogType = 0;
            _showConfirmDialog = false;
            await PopupService.EnqueueSnackbarAsync("Confirm Job Id Error", AlertTypes.Error);
            return;
        }

        switch (_confirmDialogType)
        {
            case ConfirmDialogTypes.DisabledJob:
                await DisabledJob(_confirmJobId);
                break;
            case ConfirmDialogTypes.EnabledJob:
                await EnabledJob(_confirmJobId);
                break;
            default:
                await PopupService.EnqueueSnackbarAsync("Confirm type eror", AlertTypes.Error);
                break;
        }
        _showConfirmDialog = false;
    }

    private void ResetQueryOptions()
    {
        QueryEndTime = null;
        QueryStartTime = null;
        _queryJobName = string.Empty;
        _queryOrigin = string.Empty;
        _queryTimeType = JobQueryTimeTypes.CreationTime;
        _queryStatus = 0;
        _queryJobType = 0;
        _page = 1;
        _pageSize = 10;
    }

    private bool CheckNotifyData(SchedulerJobDto job)
    {
        if (job == null)
        {
            return false;
        }

        if (Project == null)
        {
            return false;
        }

        if (_jobCreateType == JobCreateTypes.Api && string.IsNullOrWhiteSpace(job.Origin))
        {
            return false;
        }

        if (_jobCreateType == JobCreateTypes.Manual && !string.IsNullOrWhiteSpace(job.Origin))
        {
            return false;
        }

        if (job.BelongProjectIdentity != Project.Identity)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(_queryJobName) && !job.Name.Contains(_queryJobName))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(_queryOrigin) && !job.Origin.Contains(_queryOrigin))
        {
            return false;
        }

        if (_queryStatus != 0 && job.LastRunStatus != _queryStatus)
        {
            return false;
        }

        if (_queryJobType != 0 && job.JobType != _queryJobType)
        {
            return false;
        }

        return true;
    }

    private async Task SignalRNotifyDataHandler(SchedulerTaskDto taskDto)
    {
        var notifyJob = taskDto.Job;

        if (CheckNotifyData(notifyJob))
        {
            var jobIndex = _jobs.FindIndex(j => j.Id == notifyJob.Id);

            if (jobIndex > -1)
            {
                var job = _jobs.ElementAt(jobIndex);
                notifyJob.UserName = job.UserName;
                notifyJob.Avator = job.Avator;
                _jobs[jobIndex] = notifyJob;
            }
            else if (Page == 1)
            {
                var sameOwnerJob = _jobs.FirstOrDefault(j => j.OwnerId == notifyJob.OwnerId);

                if (sameOwnerJob != null)
                {
                    notifyJob.UserName = sameOwnerJob.UserName;
                    notifyJob.Avator = sameOwnerJob.Avator;
                }
                else
                {
                    var userInfo = await SchedulerServerCaller.AuthService.GetUserInfoAsync(notifyJob.OwnerId);
                    notifyJob.UserName = userInfo.Name;
                    notifyJob.Avator = userInfo.Avatar;
                }

                _jobs.Add(notifyJob);

                _jobs = _jobs.OrderByDescending(p => p.ModificationTime).ThenByDescending(p => p.CreationTime).Take(_pageSize).ToList();
            }

            StateHasChanged();
        }
    }

    public Task OnProjectChangedAsync(ProjectDto project)
    {
        Project = project;
        return Task.CompletedTask;
    }
}
