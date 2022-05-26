// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Model;

public class WorkerModel : BaseServiceModel
{
    public int CurrentRunTaskCount { get; set; }

    public string GetWorkerHost()
    {
        return GetServiceUrl(false);
    }
}
