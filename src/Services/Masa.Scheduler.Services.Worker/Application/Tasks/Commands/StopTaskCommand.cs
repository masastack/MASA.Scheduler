// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Application.Tasks.Commands;

public record StopTaskCommand(StopTaskRequest Request) : Command;
