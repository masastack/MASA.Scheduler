// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Model;

public class WorkerModel
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }

    public DateTimeOffset LastResponseTime { get; set; }

    public bool IsRunning { get; set; }

    public int NotResponseCount { get; set; }

    public int CurrentRunTaskCount { get; set; }
}


