// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Events;

public record StopTaskDomainEvent(StopSchedulerTaskRequest Request, bool IsRestart = false) : Event;
