// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Dtos;

public class SchedulerJobAppConfigDto
{
    public string JobAppIdentity { get; set; } = string.Empty;

    public string JobEntryAssembly { get; set; } = string.Empty;

    public string JobEntryMethod { get; set; } = string.Empty;

    public string JobParams { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public SchedulerResourceDto? SchedulerResourceDto { get; set; }
}

