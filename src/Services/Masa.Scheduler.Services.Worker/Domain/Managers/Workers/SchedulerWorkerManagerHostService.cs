// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Managers.Workers;

public class SchedulerWorkerManagerHostService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    public SchedulerWorkerManagerHostService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var schedulerServerManager = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<SchedulerWorkerManager>();
        await schedulerServerManager.StartManagerAsync();
    }
}
