// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Requests;

public class AddSchedulerJobRequest: BaseRequest
{
    public SchedulerJobDto Data { get; set; } = new();
}

