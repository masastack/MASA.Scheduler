// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Infrastructure.Managers;

public class BaseSchedulerManagerData<T> where T : BaseServiceModel
{
    public Guid ServiceId { get; private set; }

    public BaseSchedulerManagerData()
    {
        ServiceId = Guid.NewGuid();
    }

    public List<T> ServiceList { get; set; } = new();

    public List<string> AddressList { get; set; } = new();
}
