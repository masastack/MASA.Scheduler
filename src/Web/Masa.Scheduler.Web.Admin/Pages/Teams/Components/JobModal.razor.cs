﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.Teams.Components;

public partial class JobModal
{
    [Parameter]
    public bool Visible
    {
        get
        {
            return _visible;
        }
        set
        {
            _visible = value;
        }
    }

    [Parameter]
    public SchedulerJobDto Model
    {
        get
        {
            return _model;
        }
        set
        {
            if(_model.Id != value.Id || _model.BelongProjectIdentity != value.BelongProjectIdentity)
            {
                _model = value;

                OnModelChange();
            }
        }
    }

    [Parameter]
    public EventCallback<bool> VisibleChanged { get; set; }

    [Parameter]
    public ProjectDto Project { get; set; } = default!;

    [Parameter]
    public EventCallback<Task> OnAfterSubmit { get; set; }

    private List<WorkerModel> _workerList = new();

    private bool _visible;

    private SchedulerJobDto _model = new();

    private MForm? Form { get; set; }

    private int _step = 1;

    private HttpParameterTypes _httpParameterTypes = HttpParameterTypes.Parameter;

    private ResourceVersionTypes _resourceVersionType;

    private List<string> _versionList = new();

    private bool _requireCard = false;

    private bool _isAdd = false;

    protected override async Task OnInitializedAsync()
    {
        _isAdd = Model.Id == Guid.Empty;

        await GetWorkerList();

        await base.OnInitializedAsync();
    }

    private Task HandleVisibleChanged()
    {
        _visible = false;
        if (VisibleChanged.HasDelegate)
        {
            VisibleChanged.InvokeAsync(_visible);
        }
        return Task.CompletedTask;
    }

    private Task NextStep(EditContext context)
    {
        var success = context.Validate();

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
        _step++;
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

    private string GetStyle(JobTypes type)
    {
        var defaultStyle = "border-style: dashed;";

        if (type == Model.JobType)
        {
            defaultStyle += "background-color:#4318FF;";
        }

        if (_requireCard)
        {
            defaultStyle += "border-color: red";
        }

        return defaultStyle;
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

        if(_resourceVersionType == ResourceVersionTypes.Latest)
        {
            Model.JobAppConfig.Version = string.Empty;
        }

        return Task.CompletedTask;
    }

    private async Task Submit(EditContext context)
    {
        if (context.Validate())
        {
            if(Model.JobType == JobTypes.Http)
            {
                Model.HttpConfig.HttpParameters.RemoveAll(p => string.IsNullOrEmpty(p.Key) && string.IsNullOrEmpty(p.Value));
                Model.HttpConfig.HttpHeaders.RemoveAll(p => string.IsNullOrEmpty(p.Key) && string.IsNullOrEmpty(p.Value));
            }

            if (_isAdd)
            {
                var request = new AddSchedulerJobRequest()
                {
                    Data = Model
                };

                await SchedulerServerCaller.SchedulerJobService.AddAsync(request);

                OpenSuccessMessage("Add job success");
            }
            else
            {
                var request = new UpdateSchedulerJobRequest()
                {
                    Data = Model
                };

                await SchedulerServerCaller.SchedulerJobService.UpdateAsync(request);

                OpenSuccessMessage("Update job success");
            }

            if (OnAfterSubmit.HasDelegate)
            {
               await OnAfterSubmit.InvokeAsync();
            }

            await HandleVisibleChanged();
        }
    }
    
    private Task OnModelChange()
    {
        _isAdd = Model.Id == Guid.Empty;
        if (_isAdd)
        {
            _step = 1;
        }
        else
        {
            _step = 2;
        }

        _resourceVersionType = string.IsNullOrEmpty(Model.JobAppConfig.Version) ? ResourceVersionTypes.Latest : ResourceVersionTypes.SpecifiedVersion;
        _requireCard = false;

        if (Form is not null)
        {
            Form.ResetValidationAsync();
        }

        if (Model.JobType == JobTypes.JobApp && !string.IsNullOrEmpty(Model.JobAppConfig.JobAppIdentity))
        {
            return GetVersionList(Model.JobAppConfig.JobAppIdentity);
        }

        return Task.CompletedTask;
    }

    private Task DaprServiceAppChange(string daprServiceAppIdentity)
    {
        if(daprServiceAppIdentity != Model.DaprServiceInvocationConfig.DaprServiceIdentity)
        {
            Model.DaprServiceInvocationConfig.DaprServiceIdentity = daprServiceAppIdentity;
        }

        return Task.CompletedTask;
    }
}

