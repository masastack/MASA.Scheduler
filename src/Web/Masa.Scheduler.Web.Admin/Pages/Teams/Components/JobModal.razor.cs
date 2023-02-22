// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.Teams.Components;

public partial class JobModal
{
    [Parameter]
    public SchedulerJobDto Model
    {
        get
        {
            return _model;
        }
        set
        {
            if (_model != value)
            {
                _model = value;

                OnModelChange();
            }
        }
    }

    [Parameter]
    public ProjectDto Project
    {
        get
        {
            return _project;
        }
        set
        {
            _project = value;

            if (_project != null)
            {
                _jobApp = Project.ProjectApps.FindAll(p => p.Type == ProjectAppTypes.Job);
            }
        }
    }

    [Parameter]
    public List<ProjectDto> AllProject
    {
        get
        {
            return _allProject;
        }
        set
        {
            if (_allProject != value)
            {
                _allProject = value;
                _serviceApp = _allProject.SelectMany(p => p.ProjectApps).Where(p => p.Type == ProjectAppTypes.Service).ToList();
            }
        }
    }

    [Parameter]
    public EventCallback<Task> OnAfterDataChange { get; set; }

    private List<WorkerModel> _workerList = new();

    private bool _visible;

    private bool _cronVisible;

    private SchedulerJobDto _model = new();

    private MForm? basicForm { get; set; }

    private MForm? httpForm { get; set; }

    private MForm? jobAppForm { get; set; }

    private MForm? daprForm { get; set; }

    private int _step = 1;

    private HttpParameterTypes _httpParameterTypes = HttpParameterTypes.Parameter;

    private ResourceVersionTypes _resourceVersionType;

    private List<string> _versionList = new();

    private bool _requireCard = false;

    private bool _isAdd = false;

    private ProjectDto _project = default!;

    private List<ProjectAppDto> _serviceApp = new();

    private List<ProjectDto> _allProject = new();

    private List<ProjectAppDto> _jobApp = new();

    private SUserAutoComplete _userAutoComplete = default!;

    private string _nextRunTimeStr = string.Empty;

    private string _tempCron = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        _isAdd = Model.Id == Guid.Empty;

        await GetWorkerList();

        await GetProjects();

        await base.OnInitializedAsync();
    }

    public async Task OpenModalAsync(SchedulerJobDto model)
    {
        Model = model;

        await InvokeAsync(() =>
        {
            _visible = true;
            StateHasChanged();
        });

        await ResetValidation();
    }

    private void HandleVisibleChanged(bool val)
    {
        if (!val) HandleCancel();
    }

    private void HandleCancel()
    {
        _visible = false;
    }

    private Task NextStep(FormContext context)
    {
        var success = context.Validate();

        if (Model.ScheduleType == ScheduleTypes.Cron)
        {
            var startTime = DateTimeOffset.Now;
            var cronExpression = new CronExpression(Model.CronExpression);
            var nextExcuteTime = cronExpression.GetNextValidTimeAfter(startTime);

            if (!nextExcuteTime.HasValue)
            {
                OpenWarningMessage(T("CronExpressionNotHasNextRunTime"));
                return Task.CompletedTask;
            }

            var nextExcuteTime2 = cronExpression.GetNextValidTimeAfter(nextExcuteTime.Value);

            if (!nextExcuteTime2.HasValue)
            {
                OpenWarningMessage(T("CronExpressionNotHasNextRunTime"));
                return Task.CompletedTask;
            }

            if ((nextExcuteTime2.Value - nextExcuteTime.Value).TotalSeconds < 30)
            {
                OpenWarningMessage(T("RunningIntervalTips"));
                return Task.CompletedTask;
            }
        }

        if (success)
        {
            _step++;
        }
        return Task.CompletedTask;
    }

    private Task PreviousStep()
    {
        _step--;
        return Task.CompletedTask;
    }

    private Task SelectJobType(JobTypes jobType)
    {
        Model.JobType = jobType;
        _requireCard = false;
        _step = 2;

        return Task.CompletedTask;
    }

    private async Task OnJobAppChanged(string jobAppIdentity)
    {
        if (Model.JobAppConfig.JobAppIdentity != jobAppIdentity)
        {
            Model.JobAppConfig.JobAppIdentity = jobAppIdentity;

            await GetVersionList(jobAppIdentity);
        }
    }

    private async Task GetVersionList(string jobAppIdentity)
    {
        var request = new SchedulerResourceListRequest()
        {
            JobAppIdentity = jobAppIdentity
        };

        var resourceList = await SchedulerServerCaller.SchedulerResourceService.GetListAsync(request);

        _versionList = resourceList.Data.Select(x => x.Version).ToList();
    }

    private Task OnRoutingStrategyChanged(RoutingStrategyTypes routingStrategyType)
    {
        if (Model.RoutingStrategy != routingStrategyType)
        {
            Model.RoutingStrategy = routingStrategyType;

            if (routingStrategyType == RoutingStrategyTypes.Specified)
            {
                return GetWorkerList();
            }
        }

        return Task.CompletedTask;
    }

    private async Task GetWorkerList()
    {
        _workerList = await SchedulerServerCaller.SchedulerServerManagerService.GetWorkerListAsync();
    }

    private async Task GetProjects()
    {
        await Task.Delay(100);
        var projectListResponse = await SchedulerServerCaller.PmService.GetProjectListAsync(null);
        AllProject = projectListResponse.Data;
    }

    private string GetStyle(JobTypes type)
    {
        var defaultStyle = "border-style: dashed;";

        if (type == Model.JobType)
        {
            defaultStyle += "background-color:#4318FF; color:#fff !important";
        }

        if (_requireCard)
        {
            defaultStyle += "border-color: red; color: #fff !important";
        }

        return defaultStyle;
    }

    private string GetColor(JobTypes type)
    {
        if (type == Model.JobType || _requireCard)
        {
            return "white";
        }
        else
        {
            return "black";
        }
    }

    private Task SwitchFailedStrategyType(FailedStrategyTypes type)
    {
        Model.FailedStrategy = type;
        return Task.CompletedTask;
    }

    private Task SwitchHttpParameterType(HttpParameterTypes type)
    {
        _httpParameterTypes = type;
        return Task.CompletedTask;
    }

    private Task SwitchResourceVersionType(ResourceVersionTypes resourceVersionType)
    {
        _resourceVersionType = resourceVersionType;

        if (_resourceVersionType == ResourceVersionTypes.Latest)
        {
            Model.JobAppConfig.Version = string.Empty;
        }

        return Task.CompletedTask;
    }

    private async Task Submit(FormContext context)
    {
        if (context.Validate())
        {
            if (Model.JobType == JobTypes.Http)
            {
                Model.HttpConfig.HttpParameters.RemoveAll(p => string.IsNullOrEmpty(p.Key) && string.IsNullOrEmpty(p.Value));
                Model.HttpConfig.HttpHeaders.RemoveAll(p => string.IsNullOrEmpty(p.Key) && string.IsNullOrEmpty(p.Value));
            }

            if (Model.ScheduleType == ScheduleTypes.Cron && !CronExpression.IsValidExpression(Model.CronExpression))
            {
                OpenErrorMessage(T("CronExpressionInvalid"));
                return;
            }

            if (_isAdd)
            {
                var request = new AddSchedulerJobRequest()
                {
                    Data = Model
                };

                await SchedulerServerCaller.SchedulerJobService.AddAsync(request);

                OpenSuccessMessage(T("AddJobSuccess"));
            }
            else
            {
                var request = new UpdateSchedulerJobRequest()
                {
                    Data = Model
                };

                await SchedulerServerCaller.SchedulerJobService.UpdateAsync(request);

                OpenSuccessMessage(T("UpdateJobSuccess"));
            }

            if (OnAfterDataChange.HasDelegate)
            {
                await OnAfterDataChange.InvokeAsync();
            }

            _visible = false;

            ResetForm();
        }
    }

    private async Task RemoveJobAsync()
    {
        if (Model.Enabled)
        {
            OpenErrorMessage(T("CannotDeleteEnableJob"));
            return;
        }

        await SchedulerServerCaller.SchedulerJobService.DeleteAsync(new RemoveSchedulerJobRequest() { JobId = Model.Id });

        OpenSuccessMessage(T("DeleteSuccess"));

        if (OnAfterDataChange.HasDelegate)
        {
            await OnAfterDataChange.InvokeAsync();
        }

        HandleVisibleChanged(false);
    }

    private Task ResetValidation()
    {
        if (basicForm is not null)
        {
            basicForm.ResetValidation();
        }

        if (httpForm is not null)
        {
            httpForm.ResetValidation();
        }

        if (jobAppForm is not null)
        {
            jobAppForm.ResetValidation();
        }

        if (daprForm is not null)
        {
            daprForm.ResetValidation();
        }

        _isAdd = Model.Id == Guid.Empty;
        if (_isAdd)
        {
            _step = 1;
        }
        else
        {
            _step = 2;
        }

        return Task.CompletedTask;
    }

    private void ResetForm()
    {
        Model = new();
    }

    private Task OnModelChange()
    {
        _resourceVersionType = string.IsNullOrEmpty(Model.JobAppConfig.Version) ? ResourceVersionTypes.Latest : ResourceVersionTypes.SpecifiedVersion;
        _requireCard = false;

        if (Model.ScheduleType == ScheduleTypes.Cron && !string.IsNullOrWhiteSpace(Model.CronExpression))
        {
            GetNextRunTime();
        }

        if (Model.JobType == JobTypes.JobApp && !string.IsNullOrEmpty(Model.JobAppConfig.JobAppIdentity))
        {
            return GetVersionList(Model.JobAppConfig.JobAppIdentity);
        }

        return Task.CompletedTask;
    }

    private Task DaprServiceAppChange(string daprServiceAppIdentity)
    {
        if (daprServiceAppIdentity != Model.DaprServiceInvocationConfig.DaprServiceIdentity)
        {
            Model.DaprServiceInvocationConfig.DaprServiceIdentity = daprServiceAppIdentity;
        }

        return Task.CompletedTask;
    }

    private Task OnOwnerIdChange(Guid ownerId)
    {
        Model.OwnerId = ownerId;

        var owner = _userAutoComplete.UserSelect.FirstOrDefault();

        if (owner != null && !string.IsNullOrWhiteSpace(owner.Name))
        {
            Model.Owner = owner.Name;
        }

        return Task.CompletedTask;
    }

    private Task OnScheduleTypeChanged(ScheduleTypes scheduleType)
    {
        if (Model.ScheduleType != scheduleType)
        {
            Model.ScheduleType = scheduleType;
            if (Model.ScheduleType == ScheduleTypes.Cron && !string.IsNullOrWhiteSpace(Model.CronExpression))
            {
                GetNextRunTime();
            }
        }

        return Task.CompletedTask;
    }

    private Task OnCronValueChange(string cronValue)
    {
        if (Model.CronExpression != cronValue)
        {
            Model.CronExpression = cronValue;
            GetNextRunTime();
        }

        return Task.CompletedTask;
    }

    private void GetNextRunTime(int showCount = 5)
    {
        if (!CronExpression.IsValidExpression(Model.CronExpression))
        {
            _nextRunTimeStr = T("CronExpressionNotHasNextRunTime");
            return;
        }

        var sb = new StringBuilder();

        var startTime = DateTimeOffset.Now;

        var cronExpression = new CronExpression(Model.CronExpression);

        var timezone = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(p => p.BaseUtcOffset == TimezoneOffset);

        if (timezone != null)
            cronExpression.TimeZone = timezone;

        for (int i = 0; i < showCount; i++)
        {
            var nextExcuteTime = cronExpression.GetNextValidTimeAfter(startTime);

            if (nextExcuteTime.HasValue)
            {
                startTime = nextExcuteTime.Value;
                sb.AppendLine(startTime.ToOffset(TimezoneOffset).ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }

        if (sb.Length == 0)
        {
            _nextRunTimeStr = T("CronExpressionNotHasNextRunTime");
        }
        else
        {
            _nextRunTimeStr = sb.ToString();
        }
    }

    private Task OpenCronModal()
    {
        _cronVisible = true;
        _tempCron = Model.CronExpression;
        return Task.CompletedTask;
    }

    private Task SetCronExpression()
    {
        if (CronExpression.IsValidExpression(_tempCron))
        {
            Model.CronExpression = _tempCron;
            GetNextRunTime();
            _cronVisible = false;
        }
        else
        {
            PopupService.ToastErrorAsync(T("CronExpressionInvalid"));
        }

        return Task.CompletedTask;
    }

    private string GetTitle()
    {
        if (_step == 1)
        {
            return T("PleaseSelectJobType");
        }
        else
        {
            return Model.Id == Guid.Empty ? T("Job.Add") : T("Job.Update");
        }
    }

    private async Task HandleDel()
    {
        await ConfirmAsync(T("DeletionConfirmationMessage"), RemoveJobAsync, AlertTypes.Warning);
    }
}

