// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Infrastructure.Consts;

public class ConstStrings
{
    public const string SCHEDULER_WORKER_MANAGER_API = "/api/scheduler-worker-manager";
    public const string SCHEDULER_SERVER_MANAGER_API = "/api/scheduler-server-manager";
    public const string SCHEDULER_RESOURCE_API = "api/scheduler-resource";
    public const string SCHEDULER_JOB_API = "api/scheduler-job";
    public const string SCHEDULER_TASK_API = "api/scheduler-task";
    public const string AUTH_API = "api/auth";
    public const string PM_API = "api/pm";
    public const string PUB_SUB_NAME = "pubsub";
    public const string GLOBAL_GROUP = "Global";
    public const string OSS_API = "api/oss";
    public const string JOB_ID = "JobId";
    public const string TASK_ID = "TaskId";
    public const string SCHEDULER_PRE = "Scheduler.";
    public const string TASK_QUEUE_KEY = SCHEDULER_PRE + "TaskQueue";
    public const string TASK_SET_KEY = SCHEDULER_PRE + "TaskSet";
    public const string STOP_TASK_KEY = SCHEDULER_PRE + "StopTask";
    public const string STOP_BY_MANUAL_KEY = SCHEDULER_PRE + "StopByManual";

    public static string TaskQueueKey(string environment)
    {
        return $"{environment}:{TASK_QUEUE_KEY}";
    }

    public static string TaskSetKey(string environment)
    {
        return $"{environment}:{TASK_SET_KEY}";
    }
}
