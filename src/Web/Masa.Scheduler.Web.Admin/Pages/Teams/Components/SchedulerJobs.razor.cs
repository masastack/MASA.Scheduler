// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.Teams.Components;

public partial class SchedulerJobs : ProComponentBase
{
    [Parameter]
    [SupplyParameterFromQuery(Name = "isState")]
    public bool? IsState { get; set; }

    public ProjectDto? Project { get; set; } = default;

    [Inject]
    public SchedulerJobsState SchedulerJobsState { get; set; } = default!;

    [Inject]
    public GlobalConfig StackGlobalConfig { get; set; } = default!;

    [Inject]
    public MasaUser MasaUser { get; set; } = default!;

    [Inject]
    public IMultiEnvironmentUserContext MultiEnvironmentUserContext { get; set; } = default!;

    private TaskRunStatus _lastQueryStatus;

    private JobCreateTypes _jobCreateType = JobCreateTypes.Manual;

    private long _total;

    private List<SchedulerJobDto> _jobs = new();

    private SchedulerJobDto _modalModel = new();

    private List<KeyValuePair<string, TaskRunStatus>> _queryStatusList = new();

    private List<JobQueryTimeTypes> _jobQueryTimeTypeList = new();

    private bool _showConfirmDialog;

    private string _confirmTitle = string.Empty;

    private string _confirmMessage = string.Empty;

    private ConfirmDialogTypes _confirmDialogType;

    private Guid _confirmJobId = Guid.Empty;

    private List<string> _originList = new();

    private JobModal? _jobModal;

    private bool _advanced = false;

    private List<ProjectDto> _projects = new();

    private SchedulerJobListRequest _queryParam = new() {
        QueryTimeType = JobQueryTimeTypes.CreationTime,
        Page = 1,
        PageSize = 10
    };

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

        _queryStatusList = GetEnumMap<TaskRunStatus>();
        _queryStatusList.Insert(0, new KeyValuePair<string, TaskRunStatus>(T("All"), default));

        _jobQueryTimeTypeList = Enum.GetValues<JobQueryTimeTypes>().Where(t => t == JobQueryTimeTypes.CreationTime || t == JobQueryTimeTypes.ModificationTime).ToList();

        if (IsState == true)
        {
            SetState();
        }

        await GetProjectList(MasaUser.CurrentTeamId);

        StackGlobalConfig.OnCurrentTeamChanged += CurrentTeamChanged;

        await base.OnInitializedAsync();
    }
    private void SetState()
    {
        _queryParam.FilterStatus = SchedulerJobsState.QueryStatus;
        _queryParam.JobName = SchedulerJobsState.QueryJobName;
        _queryParam.JobType = SchedulerJobsState.QueryJobType;
        _queryParam.Origin = SchedulerJobsState.QueryOrigin;
        _queryParam.BelongProjectIdentity = SchedulerJobsState.ProjectIdentity;
        _jobCreateType = SchedulerJobsState.JobCreateType;
    }

    private void CurrentTeamChanged(Guid teamId)
    {
        GetProjectList(teamId).ContinueWith(_ => InvokeAsync(StateHasChanged));
    }

    private async Task GetProjectList(Guid teamId)
    {
        if (teamId == Guid.Empty)
        {
            _projects = new();
            return;
        }

        _projects = (await SchedulerServerCaller.PmService.GetProjectListAsync(teamId)).Data;

        Project = _projects.FirstOrDefault(x => x.Identity == _queryParam.BelongProjectIdentity);
        if (Project == null)
            Project = _projects.FirstOrDefault();

        _queryParam.BelongProjectIdentity = Project?.Identity ?? string.Empty;

        await OnQueryDataChanged();
    }

    private async Task SwitchJobCreateType(JobCreateTypes jobCreateTypes)
    {
        if (_jobCreateType != jobCreateTypes)
        {
            _jobCreateType = jobCreateTypes;
            SchedulerJobsState.JobCreateType = jobCreateTypes;
            ResetQueryOptions();
            await GetProjectJobs();
        }
    }

    public async Task OnQueryDataChanged()
    {
        SchedulerJobsState.QueryStatus = _queryParam.FilterStatus;
        SchedulerJobsState.QueryJobName = _queryParam.JobName;
        SchedulerJobsState.QueryJobType = _queryParam.JobType;
        SchedulerJobsState.QueryOrigin = _queryParam.Origin;
        SchedulerJobsState.ProjectIdentity = _queryParam.BelongProjectIdentity;
        _queryParam.Page = 1;
        await GetProjectJobs();
    }

    private Task QueryTimeChanged((DateTimeOffset? queryStartTime, DateTimeOffset? queryEndTime) arg)
    {
        _queryParam.QueryStartTime = arg.queryStartTime?.UtcDateTime;
        _queryParam.QueryEndTime = arg.queryEndTime?.UtcDateTime;
        return OnQueryDataChanged();
    }

    private async Task GetProjectJobs()
    {
        if (Project == null)
        {
            _jobs = new();
            _total = 0;
            PopupService.HideProgressLinear();
            return;
        }

        PopupService.ShowProgressLinear();

        var request = new SchedulerJobListRequest()
        {
            FilterStatus = _queryParam.FilterStatus,
            IsCreatedByManual = _jobCreateType == JobCreateTypes.Manual,
            JobName = _queryParam.JobName,
            JobType = _queryParam.JobType,
            Origin = _queryParam.Origin,
            Page = _queryParam.Page,
            PageSize = _queryParam.PageSize,
            QueryEndTime = _queryParam.QueryEndTime,
            QueryStartTime = _queryParam.QueryStartTime,
            QueryTimeType = _queryParam.QueryTimeType,
            BelongProjectIdentity = _queryParam.BelongProjectIdentity,
        };

        var jobListResponse = await SchedulerServerCaller.SchedulerJobService.GetListAsync(request);

        _total = jobListResponse.Total;

        _jobs = jobListResponse.Data;

        _originList = jobListResponse.OriginList;

        PopupService.HideProgressLinear();
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

    public void HandleJobSelect(SchedulerJobDto job)
    {
        NavigationManager.NavigateTo($"/job/task/{job.Id}");
    }

    private async Task HandlerFilterStatusChange(TaskRunStatus val)
    {
        if (_lastQueryStatus == _queryParam.FilterStatus)
        {
            _queryParam.FilterStatus = 0;
        }

        _lastQueryStatus = _queryParam.FilterStatus;

        await OnQueryDataChanged();
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
        _queryParam.QueryEndTime = null;
        _queryParam.QueryStartTime = null;
        _queryParam.JobName = string.Empty;
        _queryParam.Origin = string.Empty;
        _queryParam.QueryTimeType = JobQueryTimeTypes.CreationTime;
        _queryParam.FilterStatus = 0;
        _queryParam.JobType = 0;
        _queryParam.Page = 1;
        _queryParam.PageSize = 10;
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

        if (!string.IsNullOrWhiteSpace(_queryParam.JobName) && !job.Name.Contains(_queryParam.JobName))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(_queryParam.Origin) && !job.Origin.Contains(_queryParam.Origin))
        {
            return false;
        }

        if (_queryParam.FilterStatus != 0 && job.LastRunStatus != _queryParam.FilterStatus)
        {
            return false;
        }

        if (_queryParam.JobType != 0 && job.JobType != _queryParam.JobType)
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
                notifyJob.CreatorName = job.CreatorName;
                notifyJob.ModifierName = job.ModifierName;
                _jobs[jobIndex] = notifyJob;
            }
            else if (_queryParam.Page == 1)
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

                _jobs = _jobs.OrderByDescending(p => p.ModificationTime).ThenByDescending(p => p.CreationTime).Take(_queryParam.PageSize).ToList();
            }

        }
    }

    private void ToggleAdvanced()
    {
        _advanced = !_advanced;
    }

    private async Task HandlePageChanged(int page)
    {
        _queryParam.Page = page;
        await GetProjectJobs();
    }

    private async Task HandlePageSizeChanged(int pageSize)
    {
        _queryParam.PageSize = pageSize;
        await GetProjectJobs();
    }
}
