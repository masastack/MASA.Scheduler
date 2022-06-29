// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Jobs.Commands;

public record AddSchedulerJobCommand(AddSchedulerJobRequest Request) : Command
{
    public SchedulerJobDto Result { get; set; } = new();
}
