// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.Common;

public class CacheKeys
{
    public const string CURRENT_RUN_COUNT = nameof(CURRENT_RUN_COUNT);
    public const string TASK_RETRY_COUNT = nameof(TASK_RETRY_COUNT);
    public const string SIGNALR_NOTIFY = nameof(SIGNALR_NOTIFY);
    public const string USER_QUERY = nameof(USER_QUERY);
    public const string JOB_SCHEDULE_DEDUP = nameof(JOB_SCHEDULE_DEDUP);

    public static string JobScheduleDedupKey(string environment, Guid jobId, long bucketSeconds)
        => $"{JOB_SCHEDULE_DEDUP}:{environment}:{jobId:N}:{bucketSeconds}";
}
