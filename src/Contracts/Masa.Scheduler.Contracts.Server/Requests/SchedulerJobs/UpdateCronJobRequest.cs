// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Requests.SchedulerJobs;

public class UpdateCronJobRequest : BaseRequest
{
    public Guid JobId { get; set; }

    public bool Enabled { get; set; }

    public ScheduleTypes ScheduleType { get; set; }

    public string CronExpression { get; set; } = string.Empty;
}

