// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Humanizer;
using System.Globalization;

namespace Masa.Scheduler.Web.Admin.Pages.Teams.Components;

public partial class Jobs : ProCompontentBase
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
            _project = value;

            OnProjectChanged();
        }
    }

    [Inject]
    IPopupService PopupService { get; set; } = default!;

    private ProjectDto? _project = default!;

    private int _projectId;

    private TaskRunStatus _queryStatus;

    private string _queryJobName = string.Empty;

    private JobQueryTimeTypes _queryTimeType;

    private DateTime? _queryStartTime;

    private DateTime? _queryEndTime;

    private bool _showFilter;

    private JobOriginTypes _jobOrginType;

    private int _page = 1;

    private int _pageSize  = 10;

    private long _total;

    private string _contentHeight = "300px";

    private JobTypes _queryJobType;

    private string _queryOrigin = string.Empty;

    private List<SchedulerJobDto> _jobs = new();

    private bool _modalVisible;

    private SchedulerJobDto modalModel = new();

    public List<KeyValuePair<string, JobQueryTimeTypes>> JobQueryTimeTypes { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        JobQueryTimeTypes = GetEnumMap<JobQueryTimeTypes>();
        _jobOrginType = JobOriginTypes.ManualCreate;

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

    private async Task SwitchJobOriginType(JobOriginTypes jobOriginTypes)
    {
        _jobOrginType = jobOriginTypes;

        await GetProjectJobs();
    }

    public Task ShowFilter()
    {
        _showFilter = !_showFilter;
        _contentHeight = _showFilter ? "356px" : "300px";
        return Task.CompletedTask;
    }

    public async Task OnProjectChanged()
    {
        await GetProjectJobs();
    }

    private async Task GetProjectJobs()
    {
        if (Project == null)
        {
            return;
        }

        var request = new SchedulerJobListRequest()
        {
            FilterStatus = _queryStatus,
            IsCreatedByManual = _jobOrginType == JobOriginTypes.ManualCreate,
            JobName = _queryJobName,
            JobType = _queryJobType,
            Origin = _queryOrigin,
            Page = _page,
            PageSize = _pageSize,
            QueryEndTime = _queryEndTime,
            QueryStartTime = _queryStartTime,
            QueryTimeType = _queryTimeType,
            ProjectId = Project.Id,
        };

        var jobListResponse = await SchedulerServerCaller.JobService.GetListAsync(request);

        _total = jobListResponse.Total;

        _jobs = jobListResponse.Data;

        StateHasChanged();
    }

    private string GetJobClass(TaskRunStatus runStatus)
    {
        switch (runStatus)
        {
            case TaskRunStatus.Success:
                return "job-success";
            case TaskRunStatus.Failure:
                return "job-error";
            case TaskRunStatus.Timeout:
                return "job-timeout";
            case TaskRunStatus.TimeoutSuccess:
                return "job-warning";
            default:
                return "job-normal";
        }
    }

    private string GetJobRunText(SchedulerJobDto job)
    {
        switch (job.LastRunStatus)
        {
            case TaskRunStatus.Idle:
                return T("Idle");
            case TaskRunStatus.Running:
                var runTime =(int) (DateTimeOffset.Now - job.LastRunStartTime).TotalSeconds;
                return T("AlreadyRun") + GetRunTimeDescription(runTime);
            case TaskRunStatus.Success:
            case TaskRunStatus.Failure:
            case TaskRunStatus.Timeout:
            case TaskRunStatus.TimeoutSuccess:
                return job.LastRunEndTime.Humanize(culture: new CultureInfo(LanguageProvider.CurrentLanguage)) + T(job.LastRunStatus.ToString());
        }

        return "";
    }

    private string GetRunTimeDescription(int runTime)
    {
        var min = runTime / 60;

        var second = runTime % 60;

        if(min > 0)
        {
            return $"{min}m {second}s";
        }
        else
        {
            return $"{second}s";
        }
    }

    private async Task RunJob(SchedulerJobDto dto)
    {
        var startJobRequest = new StartSchedulerJobRequest()
        {
            JobId = dto.Id,
            // todo: use login user id
            OperatorId = Guid.NewGuid()
        };

        await SchedulerServerCaller.JobService.StartJobAsync(startJobRequest);

        await PopupService.ToastSuccessAsync("Request success");
    }

    private Task AddJob()
    {
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

        await SchedulerServerCaller.JobService.ChangeEnableStatusAsync(request);

        await PopupService.ToastSuccessAsync("Request success");
    }

    private async Task EnabledJob(SchedulerJobDto job)
    {
        var request = new ChangeEnabledStatusRequest()
        {
            Id = job.Id,
            Enabled = true
        };

        await SchedulerServerCaller.JobService.ChangeEnableStatusAsync(request);

        await PopupService.ToastSuccessAsync("Request success");
    }

    public async Task OnAfterSubmit()
    {
        await GetProjectJobs();
    }
}

