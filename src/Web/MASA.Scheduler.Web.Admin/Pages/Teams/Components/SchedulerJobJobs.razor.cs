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

                OnQueryDataChanged();
            }
        }
    }

    [Parameter]
    public EventCallback<SchedulerJobDto> OnJobSelect { get; set; }

    private ProjectDto? _project = default;

    private TaskRunStatus _queryStatus;

    private TaskRunStatus _lastQueryStatus;

    private string _queryJobName = string.Empty;

    private JobQueryTimeTypes _queryTimeType;

    private DateTime? _queryStartTime;

    private DateTime? _queryEndTime;

    private bool _showFilter;

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
        _contentHeight = _showFilter ? "356px" : "300px";
        return Task.CompletedTask;
    }

    public Task OnQueryDataChanged()
    {
        Console.WriteLine("OnQueryDataChange Invoke");
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

        var request = new SchedulerJobListRequest()
        {
            FilterStatus = QueryStatus,
            IsCreatedByManual = _jobCreateType == JobCreateTypes.Manual,
            JobName = QueryJobName,
            JobType = QueryJobType,
            Origin = QueryOrigin,
            Page = Page,
            PageSize = PageSize,
            QueryEndTime = QueryEndTime,
            QueryStartTime = QueryStartTime,
            QueryTimeType = _queryTimeType,
            ProjectId = Project.Id,
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
            case TaskRunStatus.Success:
            case TaskRunStatus.Failure:
            case TaskRunStatus.Timeout:
            case TaskRunStatus.TimeoutSuccess:
                return job.LastRunEndTime.Humanize(culture: new CultureInfo(LanguageProvider.CurrentLanguage)) + T(job.LastRunStatus.ToString());
        }

        return "";
    }

    private async Task RunJob(SchedulerJobDto dto)
    {
        var startJobRequest = new StartSchedulerJobRequest()
        {
            JobId = dto.Id,
            // todo: use login user id
            OperatorId = Guid.NewGuid()
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
        modalModel.BelongProjectId = Project!.Id;
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

    private async Task DisabledJob(SchedulerJobDto job)
    {
        var request = new ChangeEnabledStatusRequest()
        {
            Id = job.Id,
            Enabled = false
        };

        await SchedulerServerCaller.SchedulerJobService.ChangeEnableStatusAsync(request);

        await PopupService.ToastSuccessAsync("Request success");

        await GetProjectJobs();
    }

    private async Task EnabledJob(SchedulerJobDto job)
    {
        var request = new ChangeEnabledStatusRequest()
        {
            Id = job.Id,
            Enabled = true
        };

        await SchedulerServerCaller.SchedulerJobService.ChangeEnableStatusAsync(request);

        await PopupService.ToastSuccessAsync("Request success");

        await GetProjectJobs();
    }

    public async Task OnAfterSubmit()
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
}
