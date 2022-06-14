// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Requests.SchedulerJobs;

public class ChangeEnabledStatusRequest : BaseRequest
{
    public Guid Id { get; set; }

    public bool Enabled { get; set; }
}

