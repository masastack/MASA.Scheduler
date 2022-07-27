// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.Teams.Components;

public partial class SchedulerJobs : ProCompontentBase
{
    [Parameter]
    public ProjectDto? Project
    {
        get
        {
            return _project;
        }
        set
        {
            if (_project?.Id != value?.Id)
            {
                _project = value;
                _projectChange = true;
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
        get
        {
            return _visible;
        }
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

    private JobQueryTimeTypes _queryTimeType;

    private DateTime? _queryStartTime;

    private DateTime? _queryEndTime;

    private bool _showFilter;

    private string _filterIcon = "mdi-filter";

    private JobCreateTypes _jobCreateType = JobCreateTypes.Manual;

    private int _page = 1;

    private int _pageSize = 10;

    private long _total;

    private string _contentHeight = "300px";

    private JobTypes _queryJobType;

    private string _queryOrigin = string.Empty;

    private List<SchedulerJobDto> _jobs = new();

    private bool _modalVisible;

    private SchedulerJobDto modalModel = new();

    private List<KeyValuePair<string, TaskRunStatus>> _queryStatusList = new();

    private bool _showConfirmDialog;

    private string _confirmTitle = string.Empty;

    private string _confirmMessage = string.Empty;

    private ConfirmDialogTypes _confirmDialogType;

    private Guid _confirmJobId = Guid.Empty;

    public List<KeyValuePair<string, JobQueryTimeTypes>> JobQueryTimeTypes { get; set; } = new();
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

    public DateTime? QueryStartTime
    {
        get => _queryStartTime; 
        set
        {
            if (_queryStartTime != value)
            {
                _queryStartTime = value;
                OnQueryDataChanged();
            }
        }
    }

    public DateTime? QueryEndTime
    {
        get => _queryEndTime;
        set
        {
            if (_queryEndTime != value)
            {
                _queryEndTime = value;
                OnQueryDataChanged();
            }
        }
    }

    private int TotalPage
    {
        get
        {
            var totalPage = (int)((_total + PageSize - 1) / PageSize);
            return totalPage == 0 ? 1 : totalPage;
        }
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

    protected override async Task OnInitializedAsync()
    {
        await MasaSignalRClient.HubConnectionBuilder();

        MasaSignalRClient.HubConnection?.On(SignalRMethodConsts.GET_NOTIFICATION, async () =>
        {
            await GetProjectJobs();
        });

        JobQueryTimeTypes = GetEnumMap<JobQueryTimeTypes>();

        _queryStatusList = GetEnumMap<TaskRunStatus>();

        _queryStatusList.RemoveAll(p=> p.Value == TaskRunStatus.WaitToRun);

        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
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

    private Task OnVisibleChanged(bool visible)
    {
        _modalVisible = visible;
        return Task.CompletedTask;
    }

    private async Task SwitchJobCreateType(JobCreateTypes jobCreateTypes)
    {
        if(_jobCreateType != jobCreateTypes)
        {
            _jobCreateType = jobCreateTypes;

            await GetProjectJobs();
        }
    }

    public Task ShowFilter()
    {
        _showFilter = !_showFilter;
        _filterIcon = _showFilter ? "mdi-filter-off" : "mdi-filter";
        _contentHeight = _showFilter ? "356px" : "300px";
        return Task.CompletedTask;
    }

    public Task OnQueryDataChanged()
    {
        return GetProjectJobs();
    }

    private async Task GetProjectJobs()
    {
        if (Project == null)
        {
            _jobs = new();
            _total = 0;
            return;
        }

        DateTimeOffset? queryEndTimeDateTimeOffset = QueryEndTime.HasValue ? new DateTimeOffset(QueryEndTime.Value, TimezoneOffset) : null;
        DateTimeOffset? queryStartTimeDateTimeOffset = QueryStartTime.HasValue ? new DateTimeOffset(QueryStartTime.Value, TimezoneOffset) : null;

        var request = new SchedulerJobListRequest()
        {
            FilterStatus = QueryStatus,
            IsCreatedByManual = _jobCreateType == JobCreateTypes.Manual,
            JobName = QueryJobName,
            JobType = QueryJobType,
            Origin = QueryOrigin,
            Page = Page,
            PageSize = PageSize,
            QueryEndTime = queryEndTimeDateTimeOffset.HasValue ? queryEndTimeDateTimeOffset.Value.ToLocalTime().DateTime : null,
            QueryStartTime = queryStartTimeDateTimeOffset.HasValue ? queryStartTimeDateTimeOffset.Value.ToLocalTime().DateTime : null,
            QueryTimeType = _queryTimeType,
            BelongProjectIdentity = Project.Identity,
        };

        var jobListResponse = await SchedulerServerCaller.SchedulerJobService.GetListAsync(request);

        _total = jobListResponse.Total;

        _jobs = jobListResponse.Data;

        StateHasChanged();
    }

    private string GetJobClass(SchedulerJobDto job)
    {
        List<string> classList = new();

        if (!job.Enabled)
        {
            classList.Add("job-disabled");
        }

        switch (job.LastRunStatus)
        {
            case TaskRunStatus.Success:
                classList.Add("job-success");
                break;
            case TaskRunStatus.Failure:
                classList.Add("job-error");
                break;
            case TaskRunStatus.Timeout:
                classList.Add("job-timeout");
                break;
            case TaskRunStatus.TimeoutSuccess:
                classList.Add("job-warning");
                break;
            default:
                classList.Add("job-normal");
                break;
        }

        return string.Join(" ", classList);
    }

    private string GetJobColor(SchedulerJobDto job)
    {
        if (!job.Enabled)
        {
            return "#a3aed0";
        }

        switch (job.LastRunStatus)
        {
            case TaskRunStatus.Success:
                return "white";
            case TaskRunStatus.Failure:
                return "#FF5252";
            case TaskRunStatus.Timeout:
                return "#FF7D00";
            case TaskRunStatus.TimeoutSuccess:
                return "#FFF8ED";
            default:
                return "white";
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
                return job.LastRunEndTime.Humanize(culture: LanguageProvider.Culture) + T(job.LastRunStatus.ToString());
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

        await PopupService.ToastSuccessAsync("Request success");

        await GetProjectJobs();
    }

    private Task AddJob()
    {
        if(Project == null)
        {
            PopupService.ToastAsync("Project is null", AlertTypes.Warning);
            return Task.CompletedTask;
        }

        modalModel = new();
        modalModel.BelongProjectIdentity = Project!.Identity;
        modalModel.BelongTeamId = Project!.TeamId;
        modalModel.Enabled = true;

        _modalVisible = true;
        return Task.CompletedTask;
    }

    private Task EditJob(SchedulerJobDto dto)
    {
        _modalVisible = true;
        modalModel = dto;

        return Task.CompletedTask;
    }

    private async Task DisabledJob(Guid jobId)
    {
        var request = new ChangeEnabledStatusRequest()
        {
            JobId = jobId,
            Enabled = false
        };

        await SchedulerServerCaller.SchedulerJobService.ChangeEnableStatusAsync(request);

        await PopupService.ToastSuccessAsync("Request success");

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

        await PopupService.ToastSuccessAsync("Request success");

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
        if(_lastQueryStatus == QueryStatus)
        {
            QueryStatus = 0;
        }

        _lastQueryStatus = QueryStatus;

        return Task.CompletedTask;
    }

    private Task OnPrevHandler()
    {
        if (Page > 1)
        {
            Page--;
        }
        return Task.CompletedTask;
    }

    private Task OnNextHandler()
    {
        if (Page < TotalPage)
        {
            Page++;
        }
        return Task.CompletedTask;
    }

    private Task OnPageSizeChanged(int pageSize)
    {
        PageSize = pageSize;
        return Task.CompletedTask;
    }

    private Task ShowDialog(ConfirmDialogTypes confirmDialogType, Guid jobId)
    {
        _showConfirmDialog = true;
        _confirmJobId = jobId;
        _confirmDialogType = confirmDialogType;
        var job = _jobs.FirstOrDefault(p => p.Id == jobId);
        if(job == null)
        {
            _showConfirmDialog = false;
            _confirmJobId = Guid.Empty;
            _confirmDialogType = 0;
            PopupService.ToastErrorAsync("JobId error");
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
                PopupService.ToastErrorAsync("Confirm type eror");
                break;
        }

        return Task.CompletedTask;
    }

    private async Task OnSure()
    {
        if (_confirmJobId == Guid.Empty)
        {
            _confirmDialogType = 0;
            _showConfirmDialog = false;
            await PopupService.ToastErrorAsync("Confirm Job Id Error");
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
                await PopupService.ToastErrorAsync("Confirm type eror");
                break;
        }
        _showConfirmDialog = false;
    }
}
