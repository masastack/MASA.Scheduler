// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Requests.SchedulerTasks;

public class AddSchedulerTaskRequest
{
    public Guid JobId { get; set; }

    public string Origin { get; set; } = string.Empty;

    public Guid RunUserId { get; set; }
}

