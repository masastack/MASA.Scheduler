// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Requests.SchedulerJobs;

public class GetSchedulerJobByIdentityRequest
{
    public string JobIdentity { get; set; } = string.Empty;

    public string ProjectIdentity { get; set; } = string.Empty;
}
