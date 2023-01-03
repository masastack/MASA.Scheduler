// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Requests.SchedulerResources;

public class AddSchedulerResourceRequest : BaseRequest
{
    public SchedulerResourceDto Data { get; set; } = new();
}

