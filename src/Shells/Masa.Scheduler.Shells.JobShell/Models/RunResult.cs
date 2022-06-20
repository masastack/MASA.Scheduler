// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Shells.JobShell.Models;

public class RunResult
{
    public Guid TaskId { get; set; }

    public bool IsSuccess { get; set; }

    public string Message { get; set; } = string.Empty;

    public object? MethodResult { get; set; }
}

