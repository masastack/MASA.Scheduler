// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Domain.Managers.Workers.TaskHandlers;

public class TaskHanlderFactory
{
    private readonly JobAppTaskHandler _jobAppTaskHandler;
    private readonly HttpTaskHandler _httpTaskHandler;
    private readonly DaprServiceInvocationTaskHanlder _daprTaskHanlder;

    public TaskHanlderFactory(JobAppTaskHandler jobAppTaskHandler, HttpTaskHandler httpTaskHandler, DaprServiceInvocationTaskHanlder daprTaskHanlder)
    {
        _jobAppTaskHandler = jobAppTaskHandler;
        _httpTaskHandler = httpTaskHandler;
        _daprTaskHanlder = daprTaskHanlder;
    }

    public ITaskHandler GetTaskHandler(JobTypes jobTypes)
    {
        switch (jobTypes)
        {
            case JobTypes.JobApp:
                return _jobAppTaskHandler;
            case JobTypes.HTTP:
                return _httpTaskHandler;
            case JobTypes.DaprServiceInvocation:
                return _daprTaskHanlder;
            default:
                throw new UserFriendlyException($"Unknow JobType: {jobTypes}");
        }
    }
}
